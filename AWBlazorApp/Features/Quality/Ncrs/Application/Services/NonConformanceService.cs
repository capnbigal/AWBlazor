using AWBlazorApp.Features.Inventory.Adjustments.Domain; using AWBlazorApp.Features.Inventory.Items.Domain; using AWBlazorApp.Features.Inventory.Locations.Domain; using AWBlazorApp.Features.Inventory.Lots.Domain; using AWBlazorApp.Features.Inventory.Outbox.Domain; using AWBlazorApp.Features.Inventory.Queue.Domain; using AWBlazorApp.Features.Inventory.Reports.Domain; using AWBlazorApp.Features.Inventory.Serials.Domain; using AWBlazorApp.Features.Inventory.Transactions.Domain; using AWBlazorApp.Features.Inventory.Types.Domain; 
using AWBlazorApp.Features.Inventory.Services;
using AWBlazorApp.Features.Quality.Capa.Domain; using AWBlazorApp.Features.Quality.Inspections.Domain; using AWBlazorApp.Features.Quality.Ncrs.Domain; using AWBlazorApp.Features.Quality.Plans.Domain; 
using AWBlazorApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.Quality.Ncrs.Application.Services;

/// <inheritdoc />
public sealed class NonConformanceService(
    IDbContextFactory<ApplicationDbContext> dbFactory,
    IInventoryService inventoryService,
    ILogger<NonConformanceService> logger) : INonConformanceService
{
    public async Task DispositionAsync(int id, NonConformanceDisposition disposition, string? notes, string? userId, CancellationToken ct)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var ncr = await db.NonConformances.FirstOrDefaultAsync(n => n.Id == id, ct)
            ?? throw new InvalidOperationException($"NonConformance {id} not found.");
        if (ncr.Status == NonConformanceStatus.Closed)
            throw new InvalidOperationException("Cannot disposition a closed NCR.");
        if (ncr.PostedTransactionId is not null)
            throw new InvalidOperationException("NCR has already posted an inventory transaction; cannot re-disposition.");

        long? txId = null;
        if (disposition == NonConformanceDisposition.Scrap)
        {
            if (ncr.LocationId is null)
                throw new InvalidOperationException("Cannot SCRAP an NCR with no LocationId.");
            var result = await inventoryService.PostTransactionAsync(new PostTransactionRequest(
                TypeCode: InventoryTransactionTypeCodes.Scrap,
                InventoryItemId: ncr.InventoryItemId,
                Quantity: ncr.Quantity,
                UnitMeasureCode: ncr.UnitMeasureCode,
                FromLocationId: ncr.LocationId,
                ToLocationId: null,
                LotId: ncr.LotId,
                SerialUnitId: null,
                FromStatus: null,
                ToStatus: null,
                ReferenceType: null,
                ReferenceId: ncr.Id,
                ReferenceLineId: null,
                Notes: $"NCR {ncr.NcrNumber}",
                CorrelationId: null,
                OccurredAt: null), userId, ct);
            txId = result.TransactionId;
            logger.LogInformation("NCR {Ncr} dispositioned Scrap → SCRAP tx {Tx}", ncr.NcrNumber, txId);
        }
        else if (disposition == NonConformanceDisposition.Quarantine)
        {
            if (ncr.LocationId is null)
                throw new InvalidOperationException("Cannot QUARANTINE an NCR with no LocationId.");
            // Same-location MOVE with FromStatus=Available → ToStatus=Quarantine. The
            // InventoryService treats this as a paired (sign=0) move and updates both
            // balance rows in one transaction.
            var result = await inventoryService.PostTransactionAsync(new PostTransactionRequest(
                TypeCode: InventoryTransactionTypeCodes.Move,
                InventoryItemId: ncr.InventoryItemId,
                Quantity: ncr.Quantity,
                UnitMeasureCode: ncr.UnitMeasureCode,
                FromLocationId: ncr.LocationId,
                ToLocationId: ncr.LocationId,
                LotId: ncr.LotId,
                SerialUnitId: null,
                FromStatus: BalanceStatus.Available,
                ToStatus: BalanceStatus.Quarantine,
                ReferenceType: null,
                ReferenceId: ncr.Id,
                ReferenceLineId: null,
                Notes: $"NCR {ncr.NcrNumber} quarantine",
                CorrelationId: null,
                OccurredAt: null), userId, ct);
            txId = result.TransactionId;
            logger.LogInformation("NCR {Ncr} dispositioned Quarantine → MOVE Available→Quarantine tx {Tx}", ncr.NcrNumber, txId);
        }
        // Rework / UseAsIs / ReturnToSupplier: no inventory transaction — handled by people.

        ncr.Disposition = disposition;
        ncr.Status = NonConformanceStatus.Dispositioned;
        ncr.DispositionedByUserId = userId;
        ncr.DispositionedAt = DateTime.UtcNow;
        ncr.DispositionNotes = notes?.Trim();
        ncr.PostedTransactionId = txId;
        ncr.ModifiedDate = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
    }
}
