using AWBlazorApp.Features.Purchasing.Domain;

namespace AWBlazorApp.Features.Purchasing.Dtos;

public sealed record PurchaseOrderDetailDto(
    int PurchaseOrderId, int PurchaseOrderDetailId, DateTime DueDate, short OrderQty,
    int ProductId, decimal UnitPrice, decimal LineTotal, decimal ReceivedQty,
    decimal RejectedQty, decimal StockedQty, DateTime ModifiedDate);

public sealed record CreatePurchaseOrderDetailRequest
{
    public int PurchaseOrderId { get; set; }
    public DateTime DueDate { get; set; }
    public short OrderQty { get; set; }
    public int ProductId { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal ReceivedQty { get; set; }
    public decimal RejectedQty { get; set; }
}

public sealed record UpdatePurchaseOrderDetailRequest
{
    public DateTime? DueDate { get; set; }
    public short? OrderQty { get; set; }
    public int? ProductId { get; set; }
    public decimal? UnitPrice { get; set; }
    public decimal? ReceivedQty { get; set; }
    public decimal? RejectedQty { get; set; }
}

public sealed record PurchaseOrderDetailAuditLogDto(
    int Id, int PurchaseOrderId, int PurchaseOrderDetailId, string Action, string? ChangedBy,
    DateTime ChangedDate, string? ChangeSummary, DateTime DueDate, short OrderQty, int ProductId,
    decimal UnitPrice, decimal LineTotal, decimal ReceivedQty, decimal RejectedQty,
    decimal StockedQty, DateTime SourceModifiedDate);

public static class PurchaseOrderDetailMappings
{
    public static PurchaseOrderDetailDto ToDto(this PurchaseOrderDetail e) => new(
        e.PurchaseOrderId, e.PurchaseOrderDetailId, e.DueDate, e.OrderQty,
        e.ProductId, e.UnitPrice, e.LineTotal, e.ReceivedQty,
        e.RejectedQty, e.StockedQty, e.ModifiedDate);

    public static PurchaseOrderDetail ToEntity(this CreatePurchaseOrderDetailRequest r) => new()
    {
        PurchaseOrderId = r.PurchaseOrderId,
        DueDate = r.DueDate,
        OrderQty = r.OrderQty,
        ProductId = r.ProductId,
        UnitPrice = r.UnitPrice,
        // LineTotal is computed by SQL Server (OrderQty * UnitPrice) — leave default.
        ReceivedQty = r.ReceivedQty,
        RejectedQty = r.RejectedQty,
        // StockedQty is computed by SQL Server (ReceivedQty - RejectedQty) — leave default.
        ModifiedDate = DateTime.UtcNow,
    };

    public static void ApplyTo(this UpdatePurchaseOrderDetailRequest r, PurchaseOrderDetail e)
    {
        if (r.DueDate.HasValue) e.DueDate = r.DueDate.Value;
        if (r.OrderQty.HasValue) e.OrderQty = r.OrderQty.Value;
        if (r.ProductId.HasValue) e.ProductId = r.ProductId.Value;
        if (r.UnitPrice.HasValue) e.UnitPrice = r.UnitPrice.Value;
        if (r.ReceivedQty.HasValue) e.ReceivedQty = r.ReceivedQty.Value;
        if (r.RejectedQty.HasValue) e.RejectedQty = r.RejectedQty.Value;
        e.ModifiedDate = DateTime.UtcNow;
    }

    public static PurchaseOrderDetailAuditLogDto ToDto(this PurchaseOrderDetailAuditLog a) => new(
        a.Id, a.PurchaseOrderId, a.PurchaseOrderDetailId, a.Action, a.ChangedBy,
        a.ChangedDate, a.ChangeSummary, a.DueDate, a.OrderQty, a.ProductId,
        a.UnitPrice, a.LineTotal, a.ReceivedQty, a.RejectedQty,
        a.StockedQty, a.SourceModifiedDate);
}
