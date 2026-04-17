namespace AWBlazorApp.Shared.Services;

/// <summary>
/// Decoupling seam between the upstream posting services (Logistics, MES) and downstream
/// modules that react to those postings (Quality, future Maintenance, etc.). Upstream services
/// inject <see cref="IEnumerable{IPostingTriggerHook}"/> and call the relevant method after a
/// successful post; downstream modules register implementations. Default: no-op (no handlers).
/// The hooks are best-effort — exceptions are caught and logged so a downstream bug doesn't
/// roll back a successful ledger write.
/// </summary>
public interface IPostingTriggerHook
{
    Task AfterGoodsReceiptPostedAsync(GoodsReceiptLinePostedContext context, CancellationToken cancellationToken);
    Task AfterShipmentPostedAsync(ShipmentLinePostedContext context, CancellationToken cancellationToken);
    Task AfterProductionRunCompletedAsync(ProductionRunCompletedContext context, CancellationToken cancellationToken);
}

/// <summary>Everything a hook needs to know about a just-posted goods-receipt line.</summary>
public sealed record GoodsReceiptLinePostedContext(
    int GoodsReceiptId, int GoodsReceiptLineId, int? PurchaseOrderId, int? VendorBusinessEntityId,
    int InventoryItemId, int ProductId, decimal Quantity, string UnitMeasureCode,
    int? LotId, long? PostedTransactionId);

public sealed record ShipmentLinePostedContext(
    int ShipmentId, int ShipmentLineId, int? SalesOrderId, int? CustomerBusinessEntityId,
    int InventoryItemId, int ProductId, decimal Quantity, string UnitMeasureCode,
    int? LotId, int? SerialUnitId, long? PostedTransactionId);

public sealed record ProductionRunCompletedContext(
    int ProductionRunId, int? WorkOrderId, int? StationId,
    int? InventoryItemId, int? ProductId, decimal QuantityProduced, decimal QuantityScrapped,
    long? WipReceiptTransactionId);
