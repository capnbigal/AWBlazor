using System.Text;
using AWBlazorApp.Features.AdventureWorks.Domain;

namespace AWBlazorApp.Features.AdventureWorks.Audit;

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
