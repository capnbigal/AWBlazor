using AWBlazorApp.Features.Sales.Domain;

namespace AWBlazorApp.Features.Sales.Models;

public sealed record SalesOrderDetailDto(
    int SalesOrderId, int SalesOrderDetailId, string? CarrierTrackingNumber,
    short OrderQty, int ProductId, int SpecialOfferId,
    decimal UnitPrice, decimal UnitPriceDiscount, decimal LineTotal,
    Guid RowGuid, DateTime ModifiedDate);

public sealed record CreateSalesOrderDetailRequest
{
    public int SalesOrderId { get; set; }
    public string? CarrierTrackingNumber { get; set; }
    public short OrderQty { get; set; }
    public int ProductId { get; set; }
    public int SpecialOfferId { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal UnitPriceDiscount { get; set; }
}

public sealed record UpdateSalesOrderDetailRequest
{
    public string? CarrierTrackingNumber { get; set; }
    public short? OrderQty { get; set; }
    public decimal? UnitPrice { get; set; }
    public decimal? UnitPriceDiscount { get; set; }
}

public sealed record SalesOrderDetailAuditLogDto(
    int Id, int SalesOrderId, int SalesOrderDetailId, string Action, string? ChangedBy, DateTime ChangedDate,
    string? ChangeSummary, string? CarrierTrackingNumber, short OrderQty, int ProductId, int SpecialOfferId,
    decimal UnitPrice, decimal UnitPriceDiscount, decimal LineTotal,
    Guid RowGuid, DateTime SourceModifiedDate);

public static class SalesOrderDetailMappings
{
    public static SalesOrderDetailDto ToDto(this SalesOrderDetail e) => new(
        e.SalesOrderId, e.SalesOrderDetailId, e.CarrierTrackingNumber,
        e.OrderQty, e.ProductId, e.SpecialOfferId,
        e.UnitPrice, e.UnitPriceDiscount, e.LineTotal,
        e.RowGuid, e.ModifiedDate);

    public static SalesOrderDetail ToEntity(this CreateSalesOrderDetailRequest r) => new()
    {
        SalesOrderId = r.SalesOrderId,
        CarrierTrackingNumber = string.IsNullOrWhiteSpace(r.CarrierTrackingNumber) ? null : r.CarrierTrackingNumber.Trim(),
        OrderQty = r.OrderQty,
        ProductId = r.ProductId,
        SpecialOfferId = r.SpecialOfferId,
        UnitPrice = r.UnitPrice,
        UnitPriceDiscount = r.UnitPriceDiscount,
        RowGuid = Guid.NewGuid(),
        ModifiedDate = DateTime.UtcNow,
    };

    public static void ApplyTo(this UpdateSalesOrderDetailRequest r, SalesOrderDetail e)
    {
        if (r.CarrierTrackingNumber is not null) e.CarrierTrackingNumber = string.IsNullOrWhiteSpace(r.CarrierTrackingNumber) ? null : r.CarrierTrackingNumber.Trim();
        if (r.OrderQty.HasValue) e.OrderQty = r.OrderQty.Value;
        if (r.UnitPrice.HasValue) e.UnitPrice = r.UnitPrice.Value;
        if (r.UnitPriceDiscount.HasValue) e.UnitPriceDiscount = r.UnitPriceDiscount.Value;
        e.ModifiedDate = DateTime.UtcNow;
    }

    public static SalesOrderDetailAuditLogDto ToDto(this SalesOrderDetailAuditLog a) => new(
        a.Id, a.SalesOrderId, a.SalesOrderDetailId, a.Action, a.ChangedBy, a.ChangedDate, a.ChangeSummary,
        a.CarrierTrackingNumber, a.OrderQty, a.ProductId, a.SpecialOfferId,
        a.UnitPrice, a.UnitPriceDiscount, a.LineTotal,
        a.RowGuid, a.SourceModifiedDate);
}
