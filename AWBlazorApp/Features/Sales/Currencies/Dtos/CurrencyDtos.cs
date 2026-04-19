using AWBlazorApp.Features.Sales.CountryRegionCurrencies.Domain; using AWBlazorApp.Features.Sales.CreditCards.Domain; using AWBlazorApp.Features.Sales.Currencies.Domain; using AWBlazorApp.Features.Sales.CurrencyRates.Domain; using AWBlazorApp.Features.Sales.Customers.Domain; using AWBlazorApp.Features.Sales.PersonCreditCards.Domain; using AWBlazorApp.Features.Sales.SalesOrderDetails.Domain; using AWBlazorApp.Features.Sales.SalesOrderHeaders.Domain; using AWBlazorApp.Features.Sales.SalesOrderHeaderSalesReasons.Domain; using AWBlazorApp.Features.Sales.SalesPeople.Domain; using AWBlazorApp.Features.Sales.SalesPersonQuotaHistories.Domain; using AWBlazorApp.Features.Sales.SalesReasons.Domain; using AWBlazorApp.Features.Sales.SalesTaxRates.Domain; using AWBlazorApp.Features.Sales.SalesTerritories.Domain; using AWBlazorApp.Features.Sales.SalesTerritoryHistories.Domain; using AWBlazorApp.Features.Sales.ShoppingCartItems.Domain; using AWBlazorApp.Features.Sales.SpecialOffers.Domain; using AWBlazorApp.Features.Sales.SpecialOfferProducts.Domain; using AWBlazorApp.Features.Sales.Stores.Domain; 

namespace AWBlazorApp.Features.Sales.Currencies.Dtos;

public sealed record CurrencyDto(string CurrencyCode, string Name, DateTime ModifiedDate);

public sealed record CreateCurrencyRequest
{
    public string? CurrencyCode { get; set; }
    public string? Name { get; set; }
}

public sealed record UpdateCurrencyRequest
{
    public string? Name { get; set; }
}

public sealed record CurrencyAuditLogDto(
    int Id, string CurrencyCode, string Action, string? ChangedBy, DateTime ChangedDate,
    string? ChangeSummary, string? Name, DateTime SourceModifiedDate);

public static class CurrencyMappings
{
    public static CurrencyDto ToDto(this Currency e) => new(e.CurrencyCode, e.Name, e.ModifiedDate);

    public static Currency ToEntity(this CreateCurrencyRequest r) => new()
    {
        CurrencyCode = (r.CurrencyCode ?? string.Empty).Trim(),
        Name = (r.Name ?? string.Empty).Trim(),
        ModifiedDate = DateTime.UtcNow,
    };

    public static void ApplyTo(this UpdateCurrencyRequest r, Currency e)
    {
        if (r.Name is not null) e.Name = r.Name.Trim();
        e.ModifiedDate = DateTime.UtcNow;
    }

    public static CurrencyAuditLogDto ToDto(this CurrencyAuditLog a) => new(
        a.Id, a.CurrencyCode, a.Action, a.ChangedBy, a.ChangedDate, a.ChangeSummary,
        a.Name, a.SourceModifiedDate);
}
