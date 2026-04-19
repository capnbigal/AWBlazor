using AWBlazorApp.Features.Sales.CountryRegionCurrencies.Domain; using AWBlazorApp.Features.Sales.CreditCards.Domain; using AWBlazorApp.Features.Sales.Currencies.Domain; using AWBlazorApp.Features.Sales.CurrencyRates.Domain; using AWBlazorApp.Features.Sales.Customers.Domain; using AWBlazorApp.Features.Sales.PersonCreditCards.Domain; using AWBlazorApp.Features.Sales.SalesOrderDetails.Domain; using AWBlazorApp.Features.Sales.SalesOrderHeaders.Domain; using AWBlazorApp.Features.Sales.SalesOrderHeaderSalesReasons.Domain; using AWBlazorApp.Features.Sales.SalesPeople.Domain; using AWBlazorApp.Features.Sales.SalesPersonQuotaHistories.Domain; using AWBlazorApp.Features.Sales.SalesReasons.Domain; using AWBlazorApp.Features.Sales.SalesTaxRates.Domain; using AWBlazorApp.Features.Sales.SalesTerritories.Domain; using AWBlazorApp.Features.Sales.SalesTerritoryHistories.Domain; using AWBlazorApp.Features.Sales.ShoppingCartItems.Domain; using AWBlazorApp.Features.Sales.SpecialOffers.Domain; using AWBlazorApp.Features.Sales.SpecialOfferProducts.Domain; using AWBlazorApp.Features.Sales.Stores.Domain; 

namespace AWBlazorApp.Features.Sales.SalesOrderHeaderSalesReasons.Dtos;

public sealed record SalesOrderHeaderSalesReasonDto(
    int SalesOrderId, int SalesReasonId, DateTime ModifiedDate);

public sealed record CreateSalesOrderHeaderSalesReasonRequest
{
    public int SalesOrderId { get; set; }
    public int SalesReasonId { get; set; }
}

/// <summary>
/// Pure junction table — there are no non-key columns to update beyond ModifiedDate, so
/// PATCH effectively just touches the row's modified-date stamp.
/// </summary>
public sealed record UpdateSalesOrderHeaderSalesReasonRequest
{
    // Reserved for future expansion. Touching this row updates ModifiedDate.
}

public sealed record SalesOrderHeaderSalesReasonAuditLogDto(
    int Id, int SalesOrderId, int SalesReasonId, string Action, string? ChangedBy, DateTime ChangedDate,
    string? ChangeSummary, DateTime SourceModifiedDate);

public static class SalesOrderHeaderSalesReasonMappings
{
    public static SalesOrderHeaderSalesReasonDto ToDto(this SalesOrderHeaderSalesReason e) => new(
        e.SalesOrderId, e.SalesReasonId, e.ModifiedDate);

    public static SalesOrderHeaderSalesReason ToEntity(this CreateSalesOrderHeaderSalesReasonRequest r) => new()
    {
        SalesOrderId = r.SalesOrderId,
        SalesReasonId = r.SalesReasonId,
        ModifiedDate = DateTime.UtcNow,
    };

    public static void ApplyTo(this UpdateSalesOrderHeaderSalesReasonRequest _, SalesOrderHeaderSalesReason e)
    {
        e.ModifiedDate = DateTime.UtcNow;
    }

    public static SalesOrderHeaderSalesReasonAuditLogDto ToDto(this SalesOrderHeaderSalesReasonAuditLog a) => new(
        a.Id, a.SalesOrderId, a.SalesReasonId, a.Action, a.ChangedBy, a.ChangedDate, a.ChangeSummary, a.SourceModifiedDate);
}
