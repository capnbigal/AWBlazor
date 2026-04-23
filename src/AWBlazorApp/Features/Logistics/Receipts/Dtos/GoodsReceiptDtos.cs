using AWBlazorApp.Features.Logistics.Receipts.Domain; using AWBlazorApp.Features.Logistics.Shipments.Domain; using AWBlazorApp.Features.Logistics.Transfers.Domain; 

namespace AWBlazorApp.Features.Logistics.Receipts.Dtos;

public sealed record GoodsReceiptDto(
    int Id, string ReceiptNumber, int? PurchaseOrderId, int? VendorBusinessEntityId,
    int ReceivedLocationId, GoodsReceiptStatus Status, DateTime ReceivedAt,
    DateTime? PostedAt, string? PostedByUserId, string? Notes, DateTime ModifiedDate);

public sealed record CreateGoodsReceiptRequest
{
    public int? PurchaseOrderId { get; set; }
    public int? VendorBusinessEntityId { get; set; }
    public int ReceivedLocationId { get; set; }
    public DateTime? ReceivedAt { get; set; }
    public string? Notes { get; set; }
}

public sealed record UpdateGoodsReceiptRequest
{
    public int? PurchaseOrderId { get; set; }
    public int? VendorBusinessEntityId { get; set; }
    public int? ReceivedLocationId { get; set; }
    public GoodsReceiptStatus? Status { get; set; }
    public DateTime? ReceivedAt { get; set; }
    public string? Notes { get; set; }
}

public sealed record GoodsReceiptLineDto(
    int Id, int GoodsReceiptId, int? PurchaseOrderDetailId, int InventoryItemId,
    decimal Quantity, string UnitMeasureCode, int? LotId, long? PostedTransactionId, DateTime ModifiedDate);

public sealed record CreateGoodsReceiptLineRequest
{
    public int GoodsReceiptId { get; set; }
    public int? PurchaseOrderDetailId { get; set; }
    public int InventoryItemId { get; set; }
    public decimal Quantity { get; set; }
    public string? UnitMeasureCode { get; set; }
    public int? LotId { get; set; }
}

public sealed record UpdateGoodsReceiptLineRequest
{
    public int? PurchaseOrderDetailId { get; set; }
    public int? InventoryItemId { get; set; }
    public decimal? Quantity { get; set; }
    public string? UnitMeasureCode { get; set; }
    public int? LotId { get; set; }
}

public static class GoodsReceiptMappings
{
    public static GoodsReceiptDto ToDto(this GoodsReceipt e) => new(
        e.Id, e.ReceiptNumber, e.PurchaseOrderId, e.VendorBusinessEntityId,
        e.ReceivedLocationId, e.Status, e.ReceivedAt, e.PostedAt, e.PostedByUserId, e.Notes, e.ModifiedDate);

    public static GoodsReceipt ToEntity(this CreateGoodsReceiptRequest r)
    {
        var now = DateTime.UtcNow;
        return new GoodsReceipt
        {
            ReceiptNumber = $"RCP-{now:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}",
            PurchaseOrderId = r.PurchaseOrderId,
            VendorBusinessEntityId = r.VendorBusinessEntityId,
            ReceivedLocationId = r.ReceivedLocationId,
            Status = GoodsReceiptStatus.Draft,
            ReceivedAt = r.ReceivedAt ?? now,
            Notes = r.Notes?.Trim(),
            ModifiedDate = now,
        };
    }

    public static void ApplyTo(this UpdateGoodsReceiptRequest r, GoodsReceipt e)
    {
        if (r.PurchaseOrderId is not null) e.PurchaseOrderId = r.PurchaseOrderId;
        if (r.VendorBusinessEntityId is not null) e.VendorBusinessEntityId = r.VendorBusinessEntityId;
        if (r.ReceivedLocationId is not null) e.ReceivedLocationId = r.ReceivedLocationId.Value;
        if (r.Status is not null) e.Status = r.Status.Value;
        if (r.ReceivedAt is not null) e.ReceivedAt = r.ReceivedAt.Value;
        if (r.Notes is not null) e.Notes = r.Notes.Trim();
        e.ModifiedDate = DateTime.UtcNow;
    }

    public static GoodsReceiptLineDto ToDto(this GoodsReceiptLine e) => new(
        e.Id, e.GoodsReceiptId, e.PurchaseOrderDetailId, e.InventoryItemId,
        e.Quantity, e.UnitMeasureCode, e.LotId, e.PostedTransactionId, e.ModifiedDate);

    public static GoodsReceiptLine ToEntity(this CreateGoodsReceiptLineRequest r) => new()
    {
        GoodsReceiptId = r.GoodsReceiptId,
        PurchaseOrderDetailId = r.PurchaseOrderDetailId,
        InventoryItemId = r.InventoryItemId,
        Quantity = r.Quantity,
        UnitMeasureCode = (r.UnitMeasureCode ?? "EA").Trim().ToUpperInvariant(),
        LotId = r.LotId,
        ModifiedDate = DateTime.UtcNow,
    };

    public static void ApplyTo(this UpdateGoodsReceiptLineRequest r, GoodsReceiptLine e)
    {
        if (r.PurchaseOrderDetailId is not null) e.PurchaseOrderDetailId = r.PurchaseOrderDetailId;
        if (r.InventoryItemId is not null) e.InventoryItemId = r.InventoryItemId.Value;
        if (r.Quantity is not null) e.Quantity = r.Quantity.Value;
        if (r.UnitMeasureCode is not null) e.UnitMeasureCode = r.UnitMeasureCode.Trim().ToUpperInvariant();
        if (r.LotId is not null) e.LotId = r.LotId;
        e.ModifiedDate = DateTime.UtcNow;
    }

    }
