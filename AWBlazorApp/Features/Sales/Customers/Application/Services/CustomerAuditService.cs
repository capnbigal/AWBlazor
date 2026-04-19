using AWBlazorApp.Shared.Audit;
using System.Text;
using AWBlazorApp.Features.Sales.CountryRegionCurrencies.Domain; using AWBlazorApp.Features.Sales.CreditCards.Domain; using AWBlazorApp.Features.Sales.Currencies.Domain; using AWBlazorApp.Features.Sales.CurrencyRates.Domain; using AWBlazorApp.Features.Sales.Customers.Domain; using AWBlazorApp.Features.Sales.PersonCreditCards.Domain; using AWBlazorApp.Features.Sales.SalesOrderDetails.Domain; using AWBlazorApp.Features.Sales.SalesOrderHeaders.Domain; using AWBlazorApp.Features.Sales.SalesOrderHeaderSalesReasons.Domain; using AWBlazorApp.Features.Sales.SalesPeople.Domain; using AWBlazorApp.Features.Sales.SalesPersonQuotaHistories.Domain; using AWBlazorApp.Features.Sales.SalesReasons.Domain; using AWBlazorApp.Features.Sales.SalesTaxRates.Domain; using AWBlazorApp.Features.Sales.SalesTerritories.Domain; using AWBlazorApp.Features.Sales.SalesTerritoryHistories.Domain; using AWBlazorApp.Features.Sales.ShoppingCartItems.Domain; using AWBlazorApp.Features.Sales.SpecialOffers.Domain; using AWBlazorApp.Features.Sales.SpecialOfferProducts.Domain; using AWBlazorApp.Features.Sales.Stores.Domain; 

namespace AWBlazorApp.Features.Sales.Customers.Application.Services;

public static class CustomerAuditService
{
    public const string ActionCreated = "Created";
    public const string ActionUpdated = "Updated";
    public const string ActionDeleted = "Deleted";

    public static Snapshot CaptureSnapshot(Customer e) => new(e);

    public static CustomerAuditLog RecordCreate(Customer e, string? by)
        => BuildLog(e, ActionCreated, by, "Created");

    public static CustomerAuditLog RecordUpdate(Snapshot before, Customer after, string? by)
        => BuildLog(after, ActionUpdated, by, BuildDiffSummary(before, after));

    public static CustomerAuditLog RecordDelete(Customer e, string? by)
        => BuildLog(e, ActionDeleted, by, "Deleted");

    private static CustomerAuditLog BuildLog(Customer e, string action, string? by, string? summary)
        => new()
        {
            CustomerId = e.Id,
            Action = action,
            ChangedBy = by,
            ChangedDate = DateTime.UtcNow,
            ChangeSummary = AuditDiffHelpers.Truncate(summary, 2048),
            PersonId = e.PersonId,
            StoreId = e.StoreId,
            TerritoryId = e.TerritoryId,
            AccountNumber = e.AccountNumber,
            RowGuid = e.RowGuid,
            SourceModifiedDate = e.ModifiedDate,
        };

    private static string BuildDiffSummary(Snapshot before, Customer after)
    {
        var sb = new StringBuilder();
        AuditDiffHelpers.AppendIfChanged(sb, "PersonId", before.PersonId, after.PersonId);
        AuditDiffHelpers.AppendIfChanged(sb, "StoreId", before.StoreId, after.StoreId);
        AuditDiffHelpers.AppendIfChanged(sb, "TerritoryId", before.TerritoryId, after.TerritoryId);
        return sb.Length == 0 ? "No changes" : sb.ToString();
    }

    public readonly record struct Snapshot(int? PersonId, int? StoreId, int? TerritoryId)
    {
        public Snapshot(Customer e) : this(e.PersonId, e.StoreId, e.TerritoryId) { }
    }
}
