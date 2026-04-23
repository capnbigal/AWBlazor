using AWBlazorApp.Features.Sales.CountryRegionCurrencies.Domain; using AWBlazorApp.Features.Sales.CreditCards.Domain; using AWBlazorApp.Features.Sales.Currencies.Domain; using AWBlazorApp.Features.Sales.CurrencyRates.Domain; using AWBlazorApp.Features.Sales.Customers.Domain; using AWBlazorApp.Features.Sales.PersonCreditCards.Domain; using AWBlazorApp.Features.Sales.SalesOrderDetails.Domain; using AWBlazorApp.Features.Sales.SalesOrderHeaders.Domain; using AWBlazorApp.Features.Sales.SalesOrderHeaderSalesReasons.Domain; using AWBlazorApp.Features.Sales.SalesPeople.Domain; using AWBlazorApp.Features.Sales.SalesPersonQuotaHistories.Domain; using AWBlazorApp.Features.Sales.SalesReasons.Domain; using AWBlazorApp.Features.Sales.SalesTaxRates.Domain; using AWBlazorApp.Features.Sales.SalesTerritories.Domain; using AWBlazorApp.Features.Sales.SalesTerritoryHistories.Domain; using AWBlazorApp.Features.Sales.ShoppingCartItems.Domain; using AWBlazorApp.Features.Sales.SpecialOffers.Domain; using AWBlazorApp.Features.Sales.SpecialOfferProducts.Domain; using AWBlazorApp.Features.Sales.Stores.Domain; 

namespace AWBlazorApp.Features.Sales.Stores.Dtos;

public sealed record StoreDto(
    int Id, string Name, int? SalesPersonId, Guid RowGuid, DateTime ModifiedDate);

public sealed record CreateStoreRequest
{
    /// <summary>PK / FK to Person.BusinessEntity. NOT identity — caller must supply.</summary>
    public int Id { get; set; }
    public string? Name { get; set; }
    public int? SalesPersonId { get; set; }
}

public sealed record UpdateStoreRequest
{
    public string? Name { get; set; }
    public int? SalesPersonId { get; set; }
}

public static class StoreMappings
{
    public static StoreDto ToDto(this Store e) => new(
        e.Id, e.Name, e.SalesPersonId, e.RowGuid, e.ModifiedDate);

    public static Store ToEntity(this CreateStoreRequest r) => new()
    {
        Id = r.Id,
        Name = (r.Name ?? string.Empty).Trim(),
        SalesPersonId = r.SalesPersonId,
        RowGuid = Guid.NewGuid(),
        ModifiedDate = DateTime.UtcNow,
    };

    public static void ApplyTo(this UpdateStoreRequest r, Store e)
    {
        if (r.Name is not null) e.Name = r.Name.Trim();
        if (r.SalesPersonId.HasValue) e.SalesPersonId = r.SalesPersonId.Value;
        e.ModifiedDate = DateTime.UtcNow;
    }

    }
