using AWBlazorApp.Shared.Audit;
using System.Text;
using AWBlazorApp.Features.Sales.CountryRegionCurrencies.Domain; using AWBlazorApp.Features.Sales.CreditCards.Domain; using AWBlazorApp.Features.Sales.Currencies.Domain; using AWBlazorApp.Features.Sales.CurrencyRates.Domain; using AWBlazorApp.Features.Sales.Customers.Domain; using AWBlazorApp.Features.Sales.PersonCreditCards.Domain; using AWBlazorApp.Features.Sales.SalesOrderDetails.Domain; using AWBlazorApp.Features.Sales.SalesOrderHeaders.Domain; using AWBlazorApp.Features.Sales.SalesOrderHeaderSalesReasons.Domain; using AWBlazorApp.Features.Sales.SalesPeople.Domain; using AWBlazorApp.Features.Sales.SalesPersonQuotaHistories.Domain; using AWBlazorApp.Features.Sales.SalesReasons.Domain; using AWBlazorApp.Features.Sales.SalesTaxRates.Domain; using AWBlazorApp.Features.Sales.SalesTerritories.Domain; using AWBlazorApp.Features.Sales.SalesTerritoryHistories.Domain; using AWBlazorApp.Features.Sales.ShoppingCartItems.Domain; using AWBlazorApp.Features.Sales.SpecialOffers.Domain; using AWBlazorApp.Features.Sales.SpecialOfferProducts.Domain; using AWBlazorApp.Features.Sales.Stores.Domain; 

namespace AWBlazorApp.Features.Sales.SalesTerritories.Application.Services;

public static class SalesTerritoryAuditService
{
    public const string ActionCreated = "Created";
    public const string ActionUpdated = "Updated";
    public const string ActionDeleted = "Deleted";

    public static Snapshot CaptureSnapshot(SalesTerritory e) => new(e);

    public static SalesTerritoryAuditLog RecordCreate(SalesTerritory e, string? by)
        => BuildLog(e, ActionCreated, by, "Created");

    public static SalesTerritoryAuditLog RecordUpdate(Snapshot before, SalesTerritory after, string? by)
        => BuildLog(after, ActionUpdated, by, BuildDiffSummary(before, after));

    public static SalesTerritoryAuditLog RecordDelete(SalesTerritory e, string? by)
        => BuildLog(e, ActionDeleted, by, "Deleted");

    private static SalesTerritoryAuditLog BuildLog(SalesTerritory e, string action, string? by, string? summary)
        => new()
        {
            SalesTerritoryId = e.Id,
            Action = action,
            ChangedBy = by,
            ChangedDate = DateTime.UtcNow,
            ChangeSummary = AuditDiffHelpers.Truncate(summary, 2048),
            Name = e.Name,
            CountryRegionCode = e.CountryRegionCode,
            GroupName = e.GroupName,
            SalesYtd = e.SalesYtd,
            SalesLastYear = e.SalesLastYear,
            CostYtd = e.CostYtd,
            CostLastYear = e.CostLastYear,
            RowGuid = e.RowGuid,
            SourceModifiedDate = e.ModifiedDate,
        };

    private static string BuildDiffSummary(Snapshot before, SalesTerritory after)
    {
        var sb = new StringBuilder();
        AuditDiffHelpers.AppendIfChanged(sb, "Name", before.Name, after.Name);
        AuditDiffHelpers.AppendIfChanged(sb, "CountryRegionCode", before.CountryRegionCode, after.CountryRegionCode);
        AuditDiffHelpers.AppendIfChanged(sb, "GroupName", before.GroupName, after.GroupName);
        AuditDiffHelpers.AppendIfChanged(sb, "SalesYtd", before.SalesYtd, after.SalesYtd);
        AuditDiffHelpers.AppendIfChanged(sb, "SalesLastYear", before.SalesLastYear, after.SalesLastYear);
        AuditDiffHelpers.AppendIfChanged(sb, "CostYtd", before.CostYtd, after.CostYtd);
        AuditDiffHelpers.AppendIfChanged(sb, "CostLastYear", before.CostLastYear, after.CostLastYear);
        return sb.Length == 0 ? "No changes" : sb.ToString();
    }

    public readonly record struct Snapshot(
        string Name, string CountryRegionCode, string GroupName,
        decimal SalesYtd, decimal SalesLastYear, decimal CostYtd, decimal CostLastYear)
    {
        public Snapshot(SalesTerritory e) : this(
            e.Name, e.CountryRegionCode, e.GroupName,
            e.SalesYtd, e.SalesLastYear, e.CostYtd, e.CostLastYear)
        { }
    }
}
