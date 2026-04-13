using System.Text;
using AWBlazorApp.Data.Entities.AdventureWorks;

namespace AWBlazorApp.Services.AdventureWorksAudit;

public static class SalesReasonAuditService
{
    public const string ActionCreated = "Created";
    public const string ActionUpdated = "Updated";
    public const string ActionDeleted = "Deleted";

    public static Snapshot CaptureSnapshot(SalesReason e) => new(e);

    public static SalesReasonAuditLog RecordCreate(SalesReason e, string? by)
        => BuildLog(e, ActionCreated, by, "Created");

    public static SalesReasonAuditLog RecordUpdate(Snapshot before, SalesReason after, string? by)
        => BuildLog(after, ActionUpdated, by, BuildDiffSummary(before, after));

    public static SalesReasonAuditLog RecordDelete(SalesReason e, string? by)
        => BuildLog(e, ActionDeleted, by, "Deleted");

    private static SalesReasonAuditLog BuildLog(SalesReason e, string action, string? by, string? summary)
        => new()
        {
            SalesReasonId = e.Id,
            Action = action,
            ChangedBy = by,
            ChangedDate = DateTime.UtcNow,
            ChangeSummary = AuditDiffHelpers.Truncate(summary, 2048),
            Name = e.Name,
            ReasonType = e.ReasonType,
            SourceModifiedDate = e.ModifiedDate,
        };

    private static string BuildDiffSummary(Snapshot before, SalesReason after)
    {
        var sb = new StringBuilder();
        AuditDiffHelpers.AppendIfChanged(sb, "Name", before.Name, after.Name);
        AuditDiffHelpers.AppendIfChanged(sb, "ReasonType", before.ReasonType, after.ReasonType);
        return sb.Length == 0 ? "No changes" : sb.ToString();
    }

    public readonly record struct Snapshot(string Name, string ReasonType)
    {
        public Snapshot(SalesReason e) : this(e.Name, e.ReasonType) { }
    }
}
