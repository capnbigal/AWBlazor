using AWBlazorApp.Shared.Audit;
using System.Text;
using AWBlazorApp.Features.Sales.CountryRegionCurrencies.Domain; using AWBlazorApp.Features.Sales.CreditCards.Domain; using AWBlazorApp.Features.Sales.Currencies.Domain; using AWBlazorApp.Features.Sales.CurrencyRates.Domain; using AWBlazorApp.Features.Sales.Customers.Domain; using AWBlazorApp.Features.Sales.PersonCreditCards.Domain; using AWBlazorApp.Features.Sales.SalesOrderDetails.Domain; using AWBlazorApp.Features.Sales.SalesOrderHeaders.Domain; using AWBlazorApp.Features.Sales.SalesOrderHeaderSalesReasons.Domain; using AWBlazorApp.Features.Sales.SalesPeople.Domain; using AWBlazorApp.Features.Sales.SalesPersonQuotaHistories.Domain; using AWBlazorApp.Features.Sales.SalesReasons.Domain; using AWBlazorApp.Features.Sales.SalesTaxRates.Domain; using AWBlazorApp.Features.Sales.SalesTerritories.Domain; using AWBlazorApp.Features.Sales.SalesTerritoryHistories.Domain; using AWBlazorApp.Features.Sales.ShoppingCartItems.Domain; using AWBlazorApp.Features.Sales.SpecialOffers.Domain; using AWBlazorApp.Features.Sales.SpecialOfferProducts.Domain; using AWBlazorApp.Features.Sales.Stores.Domain; 

namespace AWBlazorApp.Features.Sales.CreditCards.Application.Services;

public static class CreditCardAuditService
{
    public const string ActionCreated = "Created";
    public const string ActionUpdated = "Updated";
    public const string ActionDeleted = "Deleted";

    public static Snapshot CaptureSnapshot(CreditCard e) => new(e);

    public static CreditCardAuditLog RecordCreate(CreditCard e, string? by)
        => BuildLog(e, ActionCreated, by, "Created");

    public static CreditCardAuditLog RecordUpdate(Snapshot before, CreditCard after, string? by)
        => BuildLog(after, ActionUpdated, by, BuildDiffSummary(before, after));

    public static CreditCardAuditLog RecordDelete(CreditCard e, string? by)
        => BuildLog(e, ActionDeleted, by, "Deleted");

    private static CreditCardAuditLog BuildLog(CreditCard e, string action, string? by, string? summary)
        => new()
        {
            CreditCardId = e.Id,
            Action = action,
            ChangedBy = by,
            ChangedDate = DateTime.UtcNow,
            ChangeSummary = AuditDiffHelpers.Truncate(summary, 2048),
            CardType = e.CardType,
            CardNumber = e.CardNumber,
            ExpMonth = e.ExpMonth,
            ExpYear = e.ExpYear,
            SourceModifiedDate = e.ModifiedDate,
        };

    private static string BuildDiffSummary(Snapshot before, CreditCard after)
    {
        var sb = new StringBuilder();
        AuditDiffHelpers.AppendIfChanged(sb, "CardType", before.CardType, after.CardType);
        AuditDiffHelpers.AppendIfChanged(sb, "CardNumber", before.CardNumber, after.CardNumber);
        AuditDiffHelpers.AppendIfChanged(sb, "ExpMonth", before.ExpMonth, after.ExpMonth);
        AuditDiffHelpers.AppendIfChanged(sb, "ExpYear", before.ExpYear, after.ExpYear);
        return sb.Length == 0 ? "No changes" : sb.ToString();
    }

    public readonly record struct Snapshot(string CardType, string CardNumber, byte ExpMonth, short ExpYear)
    {
        public Snapshot(CreditCard e) : this(e.CardType, e.CardNumber, e.ExpMonth, e.ExpYear) { }
    }
}
