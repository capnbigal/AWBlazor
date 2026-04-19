using AWBlazorApp.Features.Mes.Runs.Dtos;
using AWBlazorApp.Features.Inventory.Adjustments.Domain; using AWBlazorApp.Features.Inventory.Items.Domain; using AWBlazorApp.Features.Inventory.Locations.Domain; using AWBlazorApp.Features.Inventory.Lots.Domain; using AWBlazorApp.Features.Inventory.Outbox.Domain; using AWBlazorApp.Features.Inventory.Queue.Domain; using AWBlazorApp.Features.Inventory.Reports.Domain; using AWBlazorApp.Features.Inventory.Serials.Domain; using AWBlazorApp.Features.Inventory.Transactions.Domain; using AWBlazorApp.Features.Inventory.Types.Domain; 
using AWBlazorApp.Features.Inventory.Services;
using AWBlazorApp.Features.Mes.Downtime.Domain; using AWBlazorApp.Features.Mes.Instructions.Domain; using AWBlazorApp.Features.Mes.Runs.Domain; 
using AWBlazorApp.Infrastructure.Persistence;
using AWBlazorApp.Shared.Services;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.Mes.Runs.Application.Services;

/// <inheritdoc />
public sealed class ProductionRunService(
    IDbContextFactory<ApplicationDbContext> dbFactory,
    IInventoryService inventoryService,
    IEnumerable<IPostingTriggerHook> triggerHooks,
    ILogger<ProductionRunService> logger) : IProductionRunService
{
    public async Task<RunTransitionResult> StartAsync(int id, string? userId, CancellationToken ct)
        => await TransitionAsync(id, ProductionRunStatus.InProgress, userId, ct,
            requireFromStatuses: [ProductionRunStatus.Draft, ProductionRunStatus.Paused],
            stampStartIfMissing: true);

    public async Task<RunTransitionResult> PauseAsync(int id, string? userId, CancellationToken ct)
        => await TransitionAsync(id, ProductionRunStatus.Paused, userId, ct,
            requireFromStatuses: [ProductionRunStatus.InProgress]);

    public async Task<RunTransitionResult> ResumeAsync(int id, string? userId, CancellationToken ct)
        => await TransitionAsync(id, ProductionRunStatus.InProgress, userId, ct,
            requireFromStatuses: [ProductionRunStatus.Paused]);

    public async Task<RunTransitionResult> CancelAsync(int id, string? userId, CancellationToken ct)
        => await TransitionAsync(id, ProductionRunStatus.Cancelled, userId, ct,
            requireFromStatuses: [ProductionRunStatus.Draft, ProductionRunStatus.InProgress, ProductionRunStatus.Paused]);

    public async Task<RunCompletionResult> CompleteAsync(
        int id, decimal qtyProduced, decimal qtyScrapped, MaterialIssueRequest? materialIssue,
        string? userId, CancellationToken ct)
    {
        if (qtyProduced < 0 || qtyScrapped < 0)
            throw new InvalidOperationException("Quantities cannot be negative.");

        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var run = await db.ProductionRuns.FirstOrDefaultAsync(r => r.Id == id, ct)
            ?? throw new InvalidOperationException($"ProductionRun {id} not found.");

        if (run.Status is ProductionRunStatus.Completed or ProductionRunStatus.Cancelled)
            throw new InvalidOperationException($"Run is {run.Status}; cannot complete.");
        if (run.StationId is null)
            throw new InvalidOperationException("Cannot complete a run with no station assigned.");

        long? wipIssueTxId = null;
        long? wipReceiptTxId = null;

        // Optional material issue: WIP_ISSUE pulls from a stock location into work-in-progress.
        // The destination is implicit — WIP isn't a real bin, so we treat WIP_ISSUE as a pure
        // outbound (sign=-1) move with the originating location as From.
        if (materialIssue is not null && materialIssue.Quantity > 0)
        {
            var issue = await inventoryService.PostTransactionAsync(new PostTransactionRequest(
                TypeCode: InventoryTransactionTypeCodes.WipIssue,
                InventoryItemId: materialIssue.InventoryItemId,
                Quantity: materialIssue.Quantity,
                UnitMeasureCode: materialIssue.UnitMeasureCode,
                FromLocationId: materialIssue.FromLocationId,
                ToLocationId: null,
                LotId: materialIssue.LotId,
                SerialUnitId: null,
                FromStatus: null,
                ToStatus: null,
                ReferenceType: TransactionReferenceKind.WorkOrder,
                ReferenceId: run.WorkOrderId,
                ReferenceLineId: null,
                Notes: $"Run {run.RunNumber}",
                CorrelationId: null,
                OccurredAt: null), userId, ct);
            wipIssueTxId = issue.TransactionId;
        }

        // WIP_RECEIPT: finished goods land at the run's station-affiliated location. We resolve
        // a target location by looking up the first InventoryLocation associated with the
        // station's OrgUnit, falling back to the org's first warehouse if none. (Real installs
        // would configure a station→putaway-location map; that's a follow-up.)
        if (qtyProduced > 0)
        {
            var targetLocation = await ResolveReceiptLocationAsync(db, run, ct);
            var stationId = run.StationId.Value;
            var workOrderItem = await ResolveWorkOrderInventoryItemIdAsync(db, run, ct);

            var receipt = await inventoryService.PostTransactionAsync(new PostTransactionRequest(
                TypeCode: InventoryTransactionTypeCodes.WipReceipt,
                InventoryItemId: workOrderItem,
                Quantity: qtyProduced,
                UnitMeasureCode: "EA",
                FromLocationId: null,
                ToLocationId: targetLocation,
                LotId: null,
                SerialUnitId: null,
                FromStatus: null,
                ToStatus: null,
                ReferenceType: TransactionReferenceKind.WorkOrder,
                ReferenceId: run.WorkOrderId,
                ReferenceLineId: null,
                Notes: $"Run {run.RunNumber}",
                CorrelationId: null,
                OccurredAt: null), userId, ct);
            wipReceiptTxId = receipt.TransactionId;
            _ = stationId; // station resolution may use this later when we add per-station putaway maps
        }

        run.Status = ProductionRunStatus.Completed;
        run.QuantityProduced = qtyProduced;
        run.QuantityScrapped = qtyScrapped;
        run.ActualEndAt = DateTime.UtcNow;
        run.PostedByUserId = userId;
        run.ModifiedDate = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);

        await NotifyRunCompletedHooksAsync(db, run, qtyProduced, qtyScrapped, wipReceiptTxId, ct);

        logger.LogInformation("Completed run {Number}: produced={Produced}, scrapped={Scrapped}",
            run.RunNumber, qtyProduced, qtyScrapped);
        return new RunCompletionResult(run.Id, run.RunNumber, qtyProduced, qtyScrapped, wipReceiptTxId, wipIssueTxId);
    }

    private async Task NotifyRunCompletedHooksAsync(
        ApplicationDbContext db, ProductionRun run, decimal qtyProduced, decimal qtyScrapped,
        long? wipReceiptTxId, CancellationToken ct)
    {
        if (!triggerHooks.Any()) return;
        // Resolve InventoryItem + ProductId via the WorkOrder's product link, when present.
        int? itemId = null;
        int? productId = null;
        if (run.WorkOrderId is { } woId)
        {
            var pid = await db.Database
                .SqlQuery<int>($"SELECT ProductID AS Value FROM Production.WorkOrder WHERE WorkOrderID = {woId}")
                .FirstOrDefaultAsync(ct);
            if (pid != 0)
            {
                productId = pid;
                itemId = await db.InventoryItems.AsNoTracking().Where(i => i.ProductId == pid)
                    .Select(i => (int?)i.Id).FirstOrDefaultAsync(ct);
            }
        }

        var ctx = new ProductionRunCompletedContext(
            run.Id, run.WorkOrderId, run.StationId, itemId, productId,
            qtyProduced, qtyScrapped, wipReceiptTxId);
        foreach (var hook in triggerHooks)
        {
            try { await hook.AfterProductionRunCompletedAsync(ctx, ct); }
            catch (Exception ex) { logger.LogWarning(ex, "Posting trigger hook {Hook} failed for run {Run}", hook.GetType().Name, run.Id); }
        }
    }

    private async Task<RunTransitionResult> TransitionAsync(
        int id, ProductionRunStatus to, string? userId, CancellationToken ct,
        ProductionRunStatus[] requireFromStatuses,
        bool stampStartIfMissing = false)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var run = await db.ProductionRuns.FirstOrDefaultAsync(r => r.Id == id, ct)
            ?? throw new InvalidOperationException($"ProductionRun {id} not found.");
        if (!requireFromStatuses.Contains(run.Status))
            throw new InvalidOperationException($"Run is {run.Status}; cannot transition to {to}.");

        run.Status = to;
        run.ModifiedDate = DateTime.UtcNow;
        if (stampStartIfMissing && run.ActualStartAt is null)
            run.ActualStartAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        _ = userId;

        logger.LogInformation("Run {Number} → {Status}", run.RunNumber, to);
        return new RunTransitionResult(run.Id, run.Status.ToString());
    }

    /// <summary>
    /// Pick a target location for the WIP_RECEIPT. Strategy: first InventoryLocation that
    /// belongs to the station's OrgUnit (Area), then any active warehouse for the org.
    /// </summary>
    private static async Task<int> ResolveReceiptLocationAsync(ApplicationDbContext db, ProductionRun run, CancellationToken ct)
    {
        if (run.StationId is null)
            throw new InvalidOperationException("Cannot resolve receipt location without a station.");

        var station = await db.Stations.AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == run.StationId, ct)
            ?? throw new InvalidOperationException($"Station {run.StationId} not found.");

        var byOrgUnit = await db.InventoryLocations.AsNoTracking()
            .Where(l => l.IsActive && l.OrgUnitId == station.OrgUnitId)
            .Select(l => (int?)l.Id).FirstOrDefaultAsync(ct);
        if (byOrgUnit.HasValue) return byOrgUnit.Value;

        var orgUnit = await db.OrgUnits.AsNoTracking().FirstAsync(o => o.Id == station.OrgUnitId, ct);
        var anyWarehouse = await db.InventoryLocations.AsNoTracking()
            .Where(l => l.IsActive && l.OrganizationId == orgUnit.OrganizationId)
            .Select(l => (int?)l.Id).FirstOrDefaultAsync(ct)
            ?? throw new InvalidOperationException("No InventoryLocation found for the station's organization.");
        return anyWarehouse;
    }

    /// <summary>
    /// Resolve which InventoryItem the WIP_RECEIPT should credit. When the run is tied to a
    /// WorkOrder, look up the WO's ProductId and find the matching InventoryItem.
    /// </summary>
    private static async Task<int> ResolveWorkOrderInventoryItemIdAsync(ApplicationDbContext db, ProductionRun run, CancellationToken ct)
    {
        if (run.WorkOrderId is null)
            throw new InvalidOperationException("Run has no WorkOrderId; cannot resolve InventoryItem for WIP_RECEIPT. Pass an explicit one for ad-hoc runs.");

        var productId = await db.Database
            .SqlQuery<int>($"SELECT ProductID AS Value FROM Production.WorkOrder WHERE WorkOrderID = {run.WorkOrderId}")
            .FirstOrDefaultAsync(ct);
        if (productId == 0)
            throw new InvalidOperationException($"Production.WorkOrder {run.WorkOrderId} not found in AdventureWorks.");

        var item = await db.InventoryItems.AsNoTracking()
            .FirstOrDefaultAsync(i => i.ProductId == productId, ct)
            ?? throw new InvalidOperationException($"No inv.InventoryItem mapped for ProductId {productId}; create one before completing the run.");
        return item.Id;
    }
}
