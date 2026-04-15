using System.Text;
using AWBlazorApp.Features.AdventureWorks.Domain;

namespace AWBlazorApp.Features.AdventureWorks.Audit;

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
