using AWBlazorApp.Features.Sales.CountryRegionCurrencies.Domain; using AWBlazorApp.Features.Sales.CreditCards.Domain; using AWBlazorApp.Features.Sales.Currencies.Domain; using AWBlazorApp.Features.Sales.CurrencyRates.Domain; using AWBlazorApp.Features.Sales.Customers.Domain; using AWBlazorApp.Features.Sales.PersonCreditCards.Domain; using AWBlazorApp.Features.Sales.SalesOrderDetails.Domain; using AWBlazorApp.Features.Sales.SalesOrderHeaders.Domain; using AWBlazorApp.Features.Sales.SalesOrderHeaderSalesReasons.Domain; using AWBlazorApp.Features.Sales.SalesPeople.Domain; using AWBlazorApp.Features.Sales.SalesPersonQuotaHistories.Domain; using AWBlazorApp.Features.Sales.SalesReasons.Domain; using AWBlazorApp.Features.Sales.SalesTaxRates.Domain; using AWBlazorApp.Features.Sales.SalesTerritories.Domain; using AWBlazorApp.Features.Sales.SalesTerritoryHistories.Domain; using AWBlazorApp.Features.Sales.ShoppingCartItems.Domain; using AWBlazorApp.Features.Sales.SpecialOffers.Domain; using AWBlazorApp.Features.Sales.SpecialOfferProducts.Domain; using AWBlazorApp.Features.Sales.Stores.Domain; 

namespace AWBlazorApp.Features.Sales.SalesOrderDetails.Dtos;

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

    }
