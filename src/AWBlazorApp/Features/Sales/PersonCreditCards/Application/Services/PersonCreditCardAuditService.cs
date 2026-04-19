using AWBlazorApp.Shared.Audit;
using AWBlazorApp.Features.Sales.CountryRegionCurrencies.Domain; using AWBlazorApp.Features.Sales.CreditCards.Domain; using AWBlazorApp.Features.Sales.Currencies.Domain; using AWBlazorApp.Features.Sales.CurrencyRates.Domain; using AWBlazorApp.Features.Sales.Customers.Domain; using AWBlazorApp.Features.Sales.PersonCreditCards.Domain; using AWBlazorApp.Features.Sales.SalesOrderDetails.Domain; using AWBlazorApp.Features.Sales.SalesOrderHeaders.Domain; using AWBlazorApp.Features.Sales.SalesOrderHeaderSalesReasons.Domain; using AWBlazorApp.Features.Sales.SalesPeople.Domain; using AWBlazorApp.Features.Sales.SalesPersonQuotaHistories.Domain; using AWBlazorApp.Features.Sales.SalesReasons.Domain; using AWBlazorApp.Features.Sales.SalesTaxRates.Domain; using AWBlazorApp.Features.Sales.SalesTerritories.Domain; using AWBlazorApp.Features.Sales.SalesTerritoryHistories.Domain; using AWBlazorApp.Features.Sales.ShoppingCartItems.Domain; using AWBlazorApp.Features.Sales.SpecialOffers.Domain; using AWBlazorApp.Features.Sales.SpecialOfferProducts.Domain; using AWBlazorApp.Features.Sales.Stores.Domain; 

namespace AWBlazorApp.Features.Sales.PersonCreditCards.Application.Services;

public static class PersonCreditCardAuditService
{
    public const string ActionCreated = "Created";
    public const string ActionUpdated = "Updated";
    public const string ActionDeleted = "Deleted";

    public static PersonCreditCardAuditLog RecordCreate(PersonCreditCard e, string? by)
        => BuildLog(e, ActionCreated, by, "Created");

    public static PersonCreditCardAuditLog RecordUpdate(PersonCreditCard e, string? by)
        => BuildLog(e, ActionUpdated, by, "Touched");

    public static PersonCreditCardAuditLog RecordDelete(PersonCreditCard e, string? by)
        => BuildLog(e, ActionDeleted, by, "Deleted");

    private static PersonCreditCardAuditLog BuildLog(
        PersonCreditCard e, string action, string? by, string? summary)
        => new()
        {
            BusinessEntityId = e.BusinessEntityId,
            CreditCardId = e.CreditCardId,
            Action = action,
            ChangedBy = by,
            ChangedDate = DateTime.UtcNow,
            ChangeSummary = AuditDiffHelpers.Truncate(summary, 2048),
            SourceModifiedDate = e.ModifiedDate,
        };
}
