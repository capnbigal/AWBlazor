using AWBlazorApp.Shared.Audit;
using System.Text;
using AWBlazorApp.Features.Sales.CountryRegionCurrencies.Domain; using AWBlazorApp.Features.Sales.CreditCards.Domain; using AWBlazorApp.Features.Sales.Currencies.Domain; using AWBlazorApp.Features.Sales.CurrencyRates.Domain; using AWBlazorApp.Features.Sales.Customers.Domain; using AWBlazorApp.Features.Sales.PersonCreditCards.Domain; using AWBlazorApp.Features.Sales.SalesOrderDetails.Domain; using AWBlazorApp.Features.Sales.SalesOrderHeaders.Domain; using AWBlazorApp.Features.Sales.SalesOrderHeaderSalesReasons.Domain; using AWBlazorApp.Features.Sales.SalesPeople.Domain; using AWBlazorApp.Features.Sales.SalesPersonQuotaHistories.Domain; using AWBlazorApp.Features.Sales.SalesReasons.Domain; using AWBlazorApp.Features.Sales.SalesTaxRates.Domain; using AWBlazorApp.Features.Sales.SalesTerritories.Domain; using AWBlazorApp.Features.Sales.SalesTerritoryHistories.Domain; using AWBlazorApp.Features.Sales.ShoppingCartItems.Domain; using AWBlazorApp.Features.Sales.SpecialOffers.Domain; using AWBlazorApp.Features.Sales.SpecialOfferProducts.Domain; using AWBlazorApp.Features.Sales.Stores.Domain; 

namespace AWBlazorApp.Features.Sales.SpecialOffers.Application.Services;

public static class SpecialOfferAuditService
{
    public const string ActionCreated = "Created";
    public const string ActionUpdated = "Updated";
    public const string ActionDeleted = "Deleted";

    public static Snapshot CaptureSnapshot(SpecialOffer e) => new(e);

    public static SpecialOfferAuditLog RecordCreate(SpecialOffer e, string? by)
        => BuildLog(e, ActionCreated, by, "Created");

    public static SpecialOfferAuditLog RecordUpdate(Snapshot before, SpecialOffer after, string? by)
        => BuildLog(after, ActionUpdated, by, BuildDiffSummary(before, after));

    public static SpecialOfferAuditLog RecordDelete(SpecialOffer e, string? by)
        => BuildLog(e, ActionDeleted, by, "Deleted");

    private static SpecialOfferAuditLog BuildLog(SpecialOffer e, string action, string? by, string? summary)
        => new()
        {
            SpecialOfferId = e.Id,
            Action = action,
            ChangedBy = by,
            ChangedDate = DateTime.UtcNow,
            ChangeSummary = AuditDiffHelpers.Truncate(summary, 2048),
            Description = e.Description,
            DiscountPct = e.DiscountPct,
            OfferType = e.OfferType,
            Category = e.Category,
            StartDate = e.StartDate,
            EndDate = e.EndDate,
            MinQty = e.MinQty,
            MaxQty = e.MaxQty,
            RowGuid = e.RowGuid,
            SourceModifiedDate = e.ModifiedDate,
        };

    private static string BuildDiffSummary(Snapshot before, SpecialOffer after)
    {
        var sb = new StringBuilder();
        AuditDiffHelpers.AppendIfChanged(sb, "Description", before.Description, after.Description);
        AuditDiffHelpers.AppendIfChanged(sb, "DiscountPct", before.DiscountPct, after.DiscountPct);
        AuditDiffHelpers.AppendIfChanged(sb, "OfferType", before.OfferType, after.OfferType);
        AuditDiffHelpers.AppendIfChanged(sb, "Category", before.Category, after.Category);
        AuditDiffHelpers.AppendIfChanged(sb, "StartDate", before.StartDate, after.StartDate);
        AuditDiffHelpers.AppendIfChanged(sb, "EndDate", before.EndDate, after.EndDate);
        AuditDiffHelpers.AppendIfChanged(sb, "MinQty", before.MinQty, after.MinQty);
        AuditDiffHelpers.AppendIfChanged(sb, "MaxQty", before.MaxQty, after.MaxQty);
        return sb.Length == 0 ? "No changes" : sb.ToString();
    }

    public readonly record struct Snapshot(
        string Description, decimal DiscountPct, string OfferType, string Category,
        DateTime StartDate, DateTime EndDate, int MinQty, int? MaxQty)
    {
        public Snapshot(SpecialOffer e) : this(
            e.Description, e.DiscountPct, e.OfferType, e.Category,
            e.StartDate, e.EndDate, e.MinQty, e.MaxQty)
        { }
    }
}
