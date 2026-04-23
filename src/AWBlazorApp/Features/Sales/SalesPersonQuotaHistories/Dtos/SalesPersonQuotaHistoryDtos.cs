using AWBlazorApp.Features.Sales.CountryRegionCurrencies.Domain; using AWBlazorApp.Features.Sales.CreditCards.Domain; using AWBlazorApp.Features.Sales.Currencies.Domain; using AWBlazorApp.Features.Sales.CurrencyRates.Domain; using AWBlazorApp.Features.Sales.Customers.Domain; using AWBlazorApp.Features.Sales.PersonCreditCards.Domain; using AWBlazorApp.Features.Sales.SalesOrderDetails.Domain; using AWBlazorApp.Features.Sales.SalesOrderHeaders.Domain; using AWBlazorApp.Features.Sales.SalesOrderHeaderSalesReasons.Domain; using AWBlazorApp.Features.Sales.SalesPeople.Domain; using AWBlazorApp.Features.Sales.SalesPersonQuotaHistories.Domain; using AWBlazorApp.Features.Sales.SalesReasons.Domain; using AWBlazorApp.Features.Sales.SalesTaxRates.Domain; using AWBlazorApp.Features.Sales.SalesTerritories.Domain; using AWBlazorApp.Features.Sales.SalesTerritoryHistories.Domain; using AWBlazorApp.Features.Sales.ShoppingCartItems.Domain; using AWBlazorApp.Features.Sales.SpecialOffers.Domain; using AWBlazorApp.Features.Sales.SpecialOfferProducts.Domain; using AWBlazorApp.Features.Sales.Stores.Domain; 

namespace AWBlazorApp.Features.Sales.SalesPersonQuotaHistories.Dtos;

public sealed record SalesPersonQuotaHistoryDto(
    int BusinessEntityId, DateTime QuotaDate, decimal SalesQuota, Guid RowGuid, DateTime ModifiedDate);

public sealed record CreateSalesPersonQuotaHistoryRequest
{
    public int BusinessEntityId { get; set; }
    public DateTime QuotaDate { get; set; }
    public decimal SalesQuota { get; set; }
}

public sealed record UpdateSalesPersonQuotaHistoryRequest
{
    public decimal? SalesQuota { get; set; }
}

public static class SalesPersonQuotaHistoryMappings
{
    public static SalesPersonQuotaHistoryDto ToDto(this SalesPersonQuotaHistory e) => new(
        e.BusinessEntityId, e.QuotaDate, e.SalesQuota, e.RowGuid, e.ModifiedDate);

    public static SalesPersonQuotaHistory ToEntity(this CreateSalesPersonQuotaHistoryRequest r) => new()
    {
        BusinessEntityId = r.BusinessEntityId,
        QuotaDate = r.QuotaDate,
        SalesQuota = r.SalesQuota,
        RowGuid = Guid.NewGuid(),
        ModifiedDate = DateTime.UtcNow,
    };

    public static void ApplyTo(this UpdateSalesPersonQuotaHistoryRequest r, SalesPersonQuotaHistory e)
    {
        if (r.SalesQuota.HasValue) e.SalesQuota = r.SalesQuota.Value;
        e.ModifiedDate = DateTime.UtcNow;
    }

    }
