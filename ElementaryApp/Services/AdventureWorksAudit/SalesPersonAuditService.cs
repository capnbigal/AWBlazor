using System.Text;
using ElementaryApp.Data.Entities.AdventureWorks;

namespace ElementaryApp.Services.AdventureWorksAudit;

public static class SalesPersonAuditService
{
    public const string ActionCreated = "Created";
    public const string ActionUpdated = "Updated";
    public const string ActionDeleted = "Deleted";

    public static Snapshot CaptureSnapshot(SalesPerson e) => new(e);

    public static SalesPersonAuditLog RecordCreate(SalesPerson e, string? by)
        => BuildLog(e, ActionCreated, by, "Created");

    public static SalesPersonAuditLog RecordUpdate(Snapshot before, SalesPerson after, string? by)
        => BuildLog(after, ActionUpdated, by, BuildDiffSummary(before, after));

    public static SalesPersonAuditLog RecordDelete(SalesPerson e, string? by)
        => BuildLog(e, ActionDeleted, by, "Deleted");

    private static SalesPersonAuditLog BuildLog(SalesPerson e, string action, string? by, string? summary)
        => new()
        {
            SalesPersonId = e.Id,
            Action = action,
            ChangedBy = by,
            ChangedDate = DateTime.UtcNow,
            ChangeSummary = AuditDiffHelpers.Truncate(summary, 2048),
            TerritoryId = e.TerritoryId,
            SalesQuota = e.SalesQuota,
            Bonus = e.Bonus,
            CommissionPct = e.CommissionPct,
            SalesYtd = e.SalesYtd,
            SalesLastYear = e.SalesLastYear,
            RowGuid = e.RowGuid,
            SourceModifiedDate = e.ModifiedDate,
        };

    private static string BuildDiffSummary(Snapshot before, SalesPerson after)
    {
        var sb = new StringBuilder();
        AuditDiffHelpers.AppendIfChanged(sb, "TerritoryId", before.TerritoryId, after.TerritoryId);
        AuditDiffHelpers.AppendIfChanged(sb, "SalesQuota", before.SalesQuota, after.SalesQuota);
        AuditDiffHelpers.AppendIfChanged(sb, "Bonus", before.Bonus, after.Bonus);
        AuditDiffHelpers.AppendIfChanged(sb, "CommissionPct", before.CommissionPct, after.CommissionPct);
        AuditDiffHelpers.AppendIfChanged(sb, "SalesYtd", before.SalesYtd, after.SalesYtd);
        AuditDiffHelpers.AppendIfChanged(sb, "SalesLastYear", before.SalesLastYear, after.SalesLastYear);
        return sb.Length == 0 ? "No changes" : sb.ToString();
    }

    public readonly record struct Snapshot(
        int? TerritoryId, decimal? SalesQuota, decimal Bonus, decimal CommissionPct,
        decimal SalesYtd, decimal SalesLastYear)
    {
        public Snapshot(SalesPerson e) : this(
            e.TerritoryId, e.SalesQuota, e.Bonus, e.CommissionPct, e.SalesYtd, e.SalesLastYear)
        { }
    }
}
