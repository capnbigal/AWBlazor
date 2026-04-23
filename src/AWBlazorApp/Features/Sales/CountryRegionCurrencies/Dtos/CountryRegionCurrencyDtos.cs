using AWBlazorApp.Features.Sales.CountryRegionCurrencies.Domain; using AWBlazorApp.Features.Sales.CreditCards.Domain; using AWBlazorApp.Features.Sales.Currencies.Domain; using AWBlazorApp.Features.Sales.CurrencyRates.Domain; using AWBlazorApp.Features.Sales.Customers.Domain; using AWBlazorApp.Features.Sales.PersonCreditCards.Domain; using AWBlazorApp.Features.Sales.SalesOrderDetails.Domain; using AWBlazorApp.Features.Sales.SalesOrderHeaders.Domain; using AWBlazorApp.Features.Sales.SalesOrderHeaderSalesReasons.Domain; using AWBlazorApp.Features.Sales.SalesPeople.Domain; using AWBlazorApp.Features.Sales.SalesPersonQuotaHistories.Domain; using AWBlazorApp.Features.Sales.SalesReasons.Domain; using AWBlazorApp.Features.Sales.SalesTaxRates.Domain; using AWBlazorApp.Features.Sales.SalesTerritories.Domain; using AWBlazorApp.Features.Sales.SalesTerritoryHistories.Domain; using AWBlazorApp.Features.Sales.ShoppingCartItems.Domain; using AWBlazorApp.Features.Sales.SpecialOffers.Domain; using AWBlazorApp.Features.Sales.SpecialOfferProducts.Domain; using AWBlazorApp.Features.Sales.Stores.Domain; 

namespace AWBlazorApp.Features.Sales.CountryRegionCurrencies.Dtos;

public sealed record CountryRegionCurrencyDto(
    string CountryRegionCode, string CurrencyCode, DateTime ModifiedDate);

public sealed record CreateCountryRegionCurrencyRequest
{
    public string? CountryRegionCode { get; set; }
    public string? CurrencyCode { get; set; }
}

/// <summary>
/// Pure junction table — there are no non-key columns to update beyond ModifiedDate, so
/// PATCH effectively just touches the row's modified-date stamp.
/// </summary>
public sealed record UpdateCountryRegionCurrencyRequest
{
    // Reserved for future expansion. Touching this row updates ModifiedDate.
}

public static class CountryRegionCurrencyMappings
{
    public static CountryRegionCurrencyDto ToDto(this CountryRegionCurrency e) => new(
        e.CountryRegionCode, e.CurrencyCode, e.ModifiedDate);

    public static CountryRegionCurrency ToEntity(this CreateCountryRegionCurrencyRequest r) => new()
    {
        CountryRegionCode = (r.CountryRegionCode ?? string.Empty).Trim(),
        CurrencyCode = (r.CurrencyCode ?? string.Empty).Trim(),
        ModifiedDate = DateTime.UtcNow,
    };

    public static void ApplyTo(this UpdateCountryRegionCurrencyRequest _, CountryRegionCurrency e)
    {
        e.ModifiedDate = DateTime.UtcNow;
    }

    }
