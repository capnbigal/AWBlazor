using System.Text;
using ElementaryApp.Data.Entities.AdventureWorks;

namespace ElementaryApp.Services.AdventureWorksAudit;

public static class ScrapReasonAuditService
{
    public const string ActionCreated = "Created";
    public const string ActionUpdated = "Updated";
    public const string ActionDeleted = "Deleted";

    public static Snapshot CaptureSnapshot(ScrapReason e) => new(e);

    public static ScrapReasonAuditLog RecordCreate(ScrapReason e, string? by)
        => BuildLog(e, ActionCreated, by, "Created");

    public static ScrapReasonAuditLog RecordUpdate(Snapshot before, ScrapReason after, string? by)
        => BuildLog(after, ActionUpdated, by, BuildDiffSummary(before, after));

    public static ScrapReasonAuditLog RecordDelete(ScrapReason e, string? by)
        => BuildLog(e, ActionDeleted, by, "Deleted");

    private static ScrapReasonAuditLog BuildLog(ScrapReason e, string action, string? by, string? summary)
        => new()
        {
            ScrapReasonId = e.Id,
            Action = action,
            ChangedBy = by,
            ChangedDate = DateTime.UtcNow,
            ChangeSummary = AuditDiffHelpers.Truncate(summary, 2048),
            Name = e.Name,
            SourceModifiedDate = e.ModifiedDate,
        };

    private static string BuildDiffSummary(Snapshot before, ScrapReason after)
    {
        var sb = new StringBuilder();
        AuditDiffHelpers.AppendIfChanged(sb, "Name", before.Name, after.Name);
        return sb.Length == 0 ? "No changes" : sb.ToString();
    }

    public readonly record struct Snapshot(string Name)
    {
        public Snapshot(ScrapReason e) : this(e.Name) { }
    }
}
