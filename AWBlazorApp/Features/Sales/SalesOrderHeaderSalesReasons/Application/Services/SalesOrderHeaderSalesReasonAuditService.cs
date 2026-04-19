using AWBlazorApp.Shared.Audit;
using AWBlazorApp.Features.Sales.CountryRegionCurrencies.Domain; using AWBlazorApp.Features.Sales.CreditCards.Domain; using AWBlazorApp.Features.Sales.Currencies.Domain; using AWBlazorApp.Features.Sales.CurrencyRates.Domain; using AWBlazorApp.Features.Sales.Customers.Domain; using AWBlazorApp.Features.Sales.PersonCreditCards.Domain; using AWBlazorApp.Features.Sales.SalesOrderDetails.Domain; using AWBlazorApp.Features.Sales.SalesOrderHeaders.Domain; using AWBlazorApp.Features.Sales.SalesOrderHeaderSalesReasons.Domain; using AWBlazorApp.Features.Sales.SalesPeople.Domain; using AWBlazorApp.Features.Sales.SalesPersonQuotaHistories.Domain; using AWBlazorApp.Features.Sales.SalesReasons.Domain; using AWBlazorApp.Features.Sales.SalesTaxRates.Domain; using AWBlazorApp.Features.Sales.SalesTerritories.Domain; using AWBlazorApp.Features.Sales.SalesTerritoryHistories.Domain; using AWBlazorApp.Features.Sales.ShoppingCartItems.Domain; using AWBlazorApp.Features.Sales.SpecialOffers.Domain; using AWBlazorApp.Features.Sales.SpecialOfferProducts.Domain; using AWBlazorApp.Features.Sales.Stores.Domain; 

namespace AWBlazorApp.Features.Sales.SalesOrderHeaderSalesReasons.Application.Services;

public static class SalesOrderHeaderSalesReasonAuditService
{
    public const string ActionCreated = "Created";
    public const string ActionUpdated = "Updated";
    public const string ActionDeleted = "Deleted";

    public static SalesOrderHeaderSalesReasonAuditLog RecordCreate(SalesOrderHeaderSalesReason e, string? by)
        => BuildLog(e, ActionCreated, by, "Created");

    public static SalesOrderHeaderSalesReasonAuditLog RecordUpdate(SalesOrderHeaderSalesReason e, string? by)
        => BuildLog(e, ActionUpdated, by, "Touched");

    public static SalesOrderHeaderSalesReasonAuditLog RecordDelete(SalesOrderHeaderSalesReason e, string? by)
        => BuildLog(e, ActionDeleted, by, "Deleted");

    private static SalesOrderHeaderSalesReasonAuditLog BuildLog(
        SalesOrderHeaderSalesReason e, string action, string? by, string? summary)
        => new()
        {
            SalesOrderId = e.SalesOrderId,
            SalesReasonId = e.SalesReasonId,
            Action = action,
            ChangedBy = by,
            ChangedDate = DateTime.UtcNow,
            ChangeSummary = AuditDiffHelpers.Truncate(summary, 2048),
            SourceModifiedDate = e.ModifiedDate,
        };
}
