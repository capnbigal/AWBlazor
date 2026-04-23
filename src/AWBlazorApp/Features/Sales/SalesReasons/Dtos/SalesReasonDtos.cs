using AWBlazorApp.Features.Sales.CountryRegionCurrencies.Domain; using AWBlazorApp.Features.Sales.CreditCards.Domain; using AWBlazorApp.Features.Sales.Currencies.Domain; using AWBlazorApp.Features.Sales.CurrencyRates.Domain; using AWBlazorApp.Features.Sales.Customers.Domain; using AWBlazorApp.Features.Sales.PersonCreditCards.Domain; using AWBlazorApp.Features.Sales.SalesOrderDetails.Domain; using AWBlazorApp.Features.Sales.SalesOrderHeaders.Domain; using AWBlazorApp.Features.Sales.SalesOrderHeaderSalesReasons.Domain; using AWBlazorApp.Features.Sales.SalesPeople.Domain; using AWBlazorApp.Features.Sales.SalesPersonQuotaHistories.Domain; using AWBlazorApp.Features.Sales.SalesReasons.Domain; using AWBlazorApp.Features.Sales.SalesTaxRates.Domain; using AWBlazorApp.Features.Sales.SalesTerritories.Domain; using AWBlazorApp.Features.Sales.SalesTerritoryHistories.Domain; using AWBlazorApp.Features.Sales.ShoppingCartItems.Domain; using AWBlazorApp.Features.Sales.SpecialOffers.Domain; using AWBlazorApp.Features.Sales.SpecialOfferProducts.Domain; using AWBlazorApp.Features.Sales.Stores.Domain; 

namespace AWBlazorApp.Features.Sales.SalesReasons.Dtos;

public sealed record SalesReasonDto(int Id, string Name, string ReasonType, DateTime ModifiedDate);

public sealed record CreateSalesReasonRequest
{
    public string? Name { get; set; }
    public string? ReasonType { get; set; }
}

public sealed record UpdateSalesReasonRequest
{
    public string? Name { get; set; }
    public string? ReasonType { get; set; }
}

public static class SalesReasonMappings
{
    public static SalesReasonDto ToDto(this SalesReason e)
        => new(e.Id, e.Name, e.ReasonType, e.ModifiedDate);

    public static SalesReason ToEntity(this CreateSalesReasonRequest r) => new()
    {
        Name = (r.Name ?? string.Empty).Trim(),
        ReasonType = (r.ReasonType ?? string.Empty).Trim(),
        ModifiedDate = DateTime.UtcNow,
    };

    public static void ApplyTo(this UpdateSalesReasonRequest r, SalesReason e)
    {
        if (r.Name is not null) e.Name = r.Name.Trim();
        if (r.ReasonType is not null) e.ReasonType = r.ReasonType.Trim();
        e.ModifiedDate = DateTime.UtcNow;
    }

    }
