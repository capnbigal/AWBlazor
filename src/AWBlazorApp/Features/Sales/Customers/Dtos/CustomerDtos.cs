using AWBlazorApp.Features.Sales.CountryRegionCurrencies.Domain; using AWBlazorApp.Features.Sales.CreditCards.Domain; using AWBlazorApp.Features.Sales.Currencies.Domain; using AWBlazorApp.Features.Sales.CurrencyRates.Domain; using AWBlazorApp.Features.Sales.Customers.Domain; using AWBlazorApp.Features.Sales.PersonCreditCards.Domain; using AWBlazorApp.Features.Sales.SalesOrderDetails.Domain; using AWBlazorApp.Features.Sales.SalesOrderHeaders.Domain; using AWBlazorApp.Features.Sales.SalesOrderHeaderSalesReasons.Domain; using AWBlazorApp.Features.Sales.SalesPeople.Domain; using AWBlazorApp.Features.Sales.SalesPersonQuotaHistories.Domain; using AWBlazorApp.Features.Sales.SalesReasons.Domain; using AWBlazorApp.Features.Sales.SalesTaxRates.Domain; using AWBlazorApp.Features.Sales.SalesTerritories.Domain; using AWBlazorApp.Features.Sales.SalesTerritoryHistories.Domain; using AWBlazorApp.Features.Sales.ShoppingCartItems.Domain; using AWBlazorApp.Features.Sales.SpecialOffers.Domain; using AWBlazorApp.Features.Sales.SpecialOfferProducts.Domain; using AWBlazorApp.Features.Sales.Stores.Domain; 

namespace AWBlazorApp.Features.Sales.Customers.Dtos;

public sealed record CustomerDto(
    int Id, int? PersonId, int? StoreId, int? TerritoryId,
    string AccountNumber, Guid RowGuid, DateTime ModifiedDate);

public sealed record CreateCustomerRequest
{
    public int? PersonId { get; set; }
    public int? StoreId { get; set; }
    public int? TerritoryId { get; set; }
}

public sealed record UpdateCustomerRequest
{
    public int? PersonId { get; set; }
    public int? StoreId { get; set; }
    public int? TerritoryId { get; set; }
}

public static class CustomerMappings
{
    public static CustomerDto ToDto(this Customer e) => new(
        e.Id, e.PersonId, e.StoreId, e.TerritoryId, e.AccountNumber, e.RowGuid, e.ModifiedDate);

    public static Customer ToEntity(this CreateCustomerRequest r) => new()
    {
        PersonId = r.PersonId,
        StoreId = r.StoreId,
        TerritoryId = r.TerritoryId,
        RowGuid = Guid.NewGuid(),
        ModifiedDate = DateTime.UtcNow,
        // AccountNumber is computed by SQL Server — leave it empty and EF won't write it.
    };

    public static void ApplyTo(this UpdateCustomerRequest r, Customer e)
    {
        // Nullable FKs: a null in the request means "don't touch". There's no "clear it back
        // to null" channel — use a dedicated endpoint or a sentinel value if you need that.
        if (r.PersonId.HasValue) e.PersonId = r.PersonId.Value;
        if (r.StoreId.HasValue) e.StoreId = r.StoreId.Value;
        if (r.TerritoryId.HasValue) e.TerritoryId = r.TerritoryId.Value;
        e.ModifiedDate = DateTime.UtcNow;
    }

    }
