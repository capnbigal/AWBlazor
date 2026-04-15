using System.Text;
using AWBlazorApp.Features.AdventureWorks.Domain;

namespace AWBlazorApp.Features.AdventureWorks.Audit;

public static class CultureAuditService
{
    public const string ActionCreated = "Created";
    public const string ActionUpdated = "Updated";
    public const string ActionDeleted = "Deleted";

    public static Snapshot CaptureSnapshot(Culture e) => new(e);

    public static CultureAuditLog RecordCreate(Culture e, string? by)
        => BuildLog(e, ActionCreated, by, "Created");

    public static CultureAuditLog RecordUpdate(Snapshot before, Culture after, string? by)
        => BuildLog(after, ActionUpdated, by, BuildDiffSummary(before, after));

    public static CultureAuditLog RecordDelete(Culture e, string? by)
        => BuildLog(e, ActionDeleted, by, "Deleted");

    private static CultureAuditLog BuildLog(Culture e, string action, string? by, string? summary)
        => new()
        {
            CultureId = e.CultureId,
            Action = action,
            ChangedBy = by,
            ChangedDate = DateTime.UtcNow,
            ChangeSummary = AuditDiffHelpers.Truncate(summary, 2048),
            Name = e.Name,
            SourceModifiedDate = e.ModifiedDate,
        };

    private static string BuildDiffSummary(Snapshot before, Culture after)
    {
        var sb = new StringBuilder();
        AuditDiffHelpers.AppendIfChanged(sb, "Name", before.Name, after.Name);
        return sb.Length == 0 ? "No changes" : sb.ToString();
    }

    public readonly record struct Snapshot(string Name)
    {
        public Snapshot(Culture e) : this(e.Name) { }
    }
}
