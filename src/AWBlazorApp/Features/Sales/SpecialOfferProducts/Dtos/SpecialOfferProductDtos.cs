using AWBlazorApp.Features.Sales.CountryRegionCurrencies.Domain; using AWBlazorApp.Features.Sales.CreditCards.Domain; using AWBlazorApp.Features.Sales.Currencies.Domain; using AWBlazorApp.Features.Sales.CurrencyRates.Domain; using AWBlazorApp.Features.Sales.Customers.Domain; using AWBlazorApp.Features.Sales.PersonCreditCards.Domain; using AWBlazorApp.Features.Sales.SalesOrderDetails.Domain; using AWBlazorApp.Features.Sales.SalesOrderHeaders.Domain; using AWBlazorApp.Features.Sales.SalesOrderHeaderSalesReasons.Domain; using AWBlazorApp.Features.Sales.SalesPeople.Domain; using AWBlazorApp.Features.Sales.SalesPersonQuotaHistories.Domain; using AWBlazorApp.Features.Sales.SalesReasons.Domain; using AWBlazorApp.Features.Sales.SalesTaxRates.Domain; using AWBlazorApp.Features.Sales.SalesTerritories.Domain; using AWBlazorApp.Features.Sales.SalesTerritoryHistories.Domain; using AWBlazorApp.Features.Sales.ShoppingCartItems.Domain; using AWBlazorApp.Features.Sales.SpecialOffers.Domain; using AWBlazorApp.Features.Sales.SpecialOfferProducts.Domain; using AWBlazorApp.Features.Sales.Stores.Domain; 

namespace AWBlazorApp.Features.Sales.SpecialOfferProducts.Dtos;

public sealed record SpecialOfferProductDto(
    int SpecialOfferId, int ProductId, Guid RowGuid, DateTime ModifiedDate);

public sealed record CreateSpecialOfferProductRequest
{
    public int SpecialOfferId { get; set; }
    public int ProductId { get; set; }
}

/// <summary>
/// Pure junction table — there are no non-key columns to update beyond ModifiedDate, so
/// PATCH effectively just touches the row's modified-date stamp.
/// </summary>
public sealed record UpdateSpecialOfferProductRequest
{
    // Reserved for future expansion. Touching this row updates ModifiedDate.
}

public static class SpecialOfferProductMappings
{
    public static SpecialOfferProductDto ToDto(this SpecialOfferProduct e) => new(
        e.SpecialOfferId, e.ProductId, e.RowGuid, e.ModifiedDate);

    public static SpecialOfferProduct ToEntity(this CreateSpecialOfferProductRequest r) => new()
    {
        SpecialOfferId = r.SpecialOfferId,
        ProductId = r.ProductId,
        RowGuid = Guid.NewGuid(),
        ModifiedDate = DateTime.UtcNow,
    };

    public static void ApplyTo(this UpdateSpecialOfferProductRequest _, SpecialOfferProduct e)
    {
        e.ModifiedDate = DateTime.UtcNow;
    }

    }
