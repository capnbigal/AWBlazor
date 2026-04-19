using AWBlazorApp.Features.Quality.Capa.Domain; using AWBlazorApp.Features.Quality.Inspections.Domain; using AWBlazorApp.Features.Quality.Ncrs.Domain; using AWBlazorApp.Features.Quality.Plans.Domain; 
using AWBlazorApp.Infrastructure.Persistence;
using AWBlazorApp.Shared.Services;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.Quality.Inspections.Application.Services;

/// <summary>
/// Listens for upstream postings via <see cref="IPostingTriggerHook"/> and creates Pending
/// inspections whenever an active <see cref="InspectionPlan"/> matches. Best-effort: a query
/// failure here does not roll back the upstream posting (caller's loop catches and logs).
/// Matching rules:
///  • Receipt: scope is Inbound or Vendor, ProductId matches, AutoTriggerOnReceipt = true.
///    For Vendor scope, VendorBusinessEntityId on the plan must match the receipt's vendor.
///  • Shipment: scope is Outbound, ProductId matches, AutoTriggerOnShipment = true.
///  • Production run: scope is InProcess, ProductId matches (when known), AutoTriggerOnProductionRun = true.
/// </summary>
public sealed class InspectionTriggerHook(
    IDbContextFactory<ApplicationDbContext> dbFactory,
    IInspectionService inspections,
    ILogger<InspectionTriggerHook> logger) : IPostingTriggerHook
{
    public async Task AfterGoodsReceiptPostedAsync(GoodsReceiptLinePostedContext ctx, CancellationToken ct)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var plans = await db.InspectionPlans.AsNoTracking()
            .Where(p => p.IsActive
                        && p.AutoTriggerOnReceipt
                        && (p.Scope == InspectionScope.Inbound || p.Scope == InspectionScope.Vendor)
                        && p.ProductId == ctx.ProductId
                        && (p.VendorBusinessEntityId == null || p.VendorBusinessEntityId == ctx.VendorBusinessEntityId))
            .ToListAsync(ct);

        foreach (var plan in plans)
        {
            await inspections.CreateAsync(new CreateInspectionInput(
                InspectionPlanId: plan.Id,
                SourceKind: InspectionSourceKind.GoodsReceiptLine,
                SourceId: ctx.GoodsReceiptLineId,
                InventoryItemId: ctx.InventoryItemId,
                LotId: ctx.LotId,
                Quantity: ctx.Quantity,
                UnitMeasureCode: ctx.UnitMeasureCode,
                Notes: $"Auto-triggered from receipt line {ctx.GoodsReceiptLineId}"),
                userId: null, ct);
            logger.LogInformation("Auto-created inspection from receipt line {Line} matching plan {Plan}",
                ctx.GoodsReceiptLineId, plan.PlanCode);
        }
    }

    public async Task AfterShipmentPostedAsync(ShipmentLinePostedContext ctx, CancellationToken ct)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var plans = await db.InspectionPlans.AsNoTracking()
            .Where(p => p.IsActive
                        && p.AutoTriggerOnShipment
                        && p.Scope == InspectionScope.Outbound
                        && p.ProductId == ctx.ProductId)
            .ToListAsync(ct);

        foreach (var plan in plans)
        {
            await inspections.CreateAsync(new CreateInspectionInput(
                plan.Id, InspectionSourceKind.ShipmentLine, ctx.ShipmentLineId,
                ctx.InventoryItemId, ctx.LotId, ctx.Quantity, ctx.UnitMeasureCode,
                $"Auto-triggered from shipment line {ctx.ShipmentLineId}"),
                userId: null, ct);
            logger.LogInformation("Auto-created inspection from shipment line {Line} matching plan {Plan}",
                ctx.ShipmentLineId, plan.PlanCode);
        }
    }

    public async Task AfterProductionRunCompletedAsync(ProductionRunCompletedContext ctx, CancellationToken ct)
    {
        if (ctx.ProductId is null || ctx.InventoryItemId is null) return;
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var plans = await db.InspectionPlans.AsNoTracking()
            .Where(p => p.IsActive
                        && p.AutoTriggerOnProductionRun
                        && p.Scope == InspectionScope.InProcess
                        && p.ProductId == ctx.ProductId)
            .ToListAsync(ct);

        foreach (var plan in plans)
        {
            await inspections.CreateAsync(new CreateInspectionInput(
                plan.Id, InspectionSourceKind.ProductionRun, ctx.ProductionRunId,
                ctx.InventoryItemId, null, ctx.QuantityProduced, "EA",
                $"Auto-triggered from production run {ctx.ProductionRunId}"),
                userId: null, ct);
            logger.LogInformation("Auto-created inspection from run {Run} matching plan {Plan}",
                ctx.ProductionRunId, plan.PlanCode);
        }
    }
}
