using AWBlazorApp.Shared.Audit;
using System.Text;
using AWBlazorApp.Features.Sales.CountryRegionCurrencies.Domain; using AWBlazorApp.Features.Sales.CreditCards.Domain; using AWBlazorApp.Features.Sales.Currencies.Domain; using AWBlazorApp.Features.Sales.CurrencyRates.Domain; using AWBlazorApp.Features.Sales.Customers.Domain; using AWBlazorApp.Features.Sales.PersonCreditCards.Domain; using AWBlazorApp.Features.Sales.SalesOrderDetails.Domain; using AWBlazorApp.Features.Sales.SalesOrderHeaders.Domain; using AWBlazorApp.Features.Sales.SalesOrderHeaderSalesReasons.Domain; using AWBlazorApp.Features.Sales.SalesPeople.Domain; using AWBlazorApp.Features.Sales.SalesPersonQuotaHistories.Domain; using AWBlazorApp.Features.Sales.SalesReasons.Domain; using AWBlazorApp.Features.Sales.SalesTaxRates.Domain; using AWBlazorApp.Features.Sales.SalesTerritories.Domain; using AWBlazorApp.Features.Sales.SalesTerritoryHistories.Domain; using AWBlazorApp.Features.Sales.ShoppingCartItems.Domain; using AWBlazorApp.Features.Sales.SpecialOffers.Domain; using AWBlazorApp.Features.Sales.SpecialOfferProducts.Domain; using AWBlazorApp.Features.Sales.Stores.Domain; 

namespace AWBlazorApp.Features.Sales.SalesPersonQuotaHistories.Application.Services;

public static class SalesPersonQuotaHistoryAuditService
{
    public const string ActionCreated = "Created";
    public const string ActionUpdated = "Updated";
    public const string ActionDeleted = "Deleted";

    public static Snapshot CaptureSnapshot(SalesPersonQuotaHistory e) => new(e);

    public static SalesPersonQuotaHistoryAuditLog RecordCreate(SalesPersonQuotaHistory e, string? by)
        => BuildLog(e, ActionCreated, by, "Created");

    public static SalesPersonQuotaHistoryAuditLog RecordUpdate(Snapshot before, SalesPersonQuotaHistory after, string? by)
        => BuildLog(after, ActionUpdated, by, BuildDiffSummary(before, after));

    public static SalesPersonQuotaHistoryAuditLog RecordDelete(SalesPersonQuotaHistory e, string? by)
        => BuildLog(e, ActionDeleted, by, "Deleted");

    private static SalesPersonQuotaHistoryAuditLog BuildLog(
        SalesPersonQuotaHistory e, string action, string? by, string? summary)
        => new()
        {
            BusinessEntityId = e.BusinessEntityId,
            QuotaDate = e.QuotaDate,
            Action = action,
            ChangedBy = by,
            ChangedDate = DateTime.UtcNow,
            ChangeSummary = AuditDiffHelpers.Truncate(summary, 2048),
            SalesQuota = e.SalesQuota,
            RowGuid = e.RowGuid,
            SourceModifiedDate = e.ModifiedDate,
        };

    private static string BuildDiffSummary(Snapshot before, SalesPersonQuotaHistory after)
    {
        var sb = new StringBuilder();
        AuditDiffHelpers.AppendIfChanged(sb, "SalesQuota", before.SalesQuota, after.SalesQuota);
        return sb.Length == 0 ? "No changes" : sb.ToString();
    }

    public readonly record struct Snapshot(decimal SalesQuota)
    {
        public Snapshot(SalesPersonQuotaHistory e) : this(e.SalesQuota) { }
    }
}
