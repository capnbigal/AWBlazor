using System.Text;
using AWBlazorApp.Features.AdventureWorks.Domain;

namespace AWBlazorApp.Features.AdventureWorks.Audit;

public static class UnitMeasureAuditService
{
    public const string ActionCreated = "Created";
    public const string ActionUpdated = "Updated";
    public const string ActionDeleted = "Deleted";

    public static Snapshot CaptureSnapshot(UnitMeasure e) => new(e);

    public static UnitMeasureAuditLog RecordCreate(UnitMeasure e, string? by)
        => BuildLog(e, ActionCreated, by, "Created");

    public static UnitMeasureAuditLog RecordUpdate(Snapshot before, UnitMeasure after, string? by)
        => BuildLog(after, ActionUpdated, by, BuildDiffSummary(before, after));

    public static UnitMeasureAuditLog RecordDelete(UnitMeasure e, string? by)
        => BuildLog(e, ActionDeleted, by, "Deleted");

    private static UnitMeasureAuditLog BuildLog(UnitMeasure e, string action, string? by, string? summary)
        => new()
        {
            UnitMeasureCode = e.UnitMeasureCode,
            Action = action,
            ChangedBy = by,
            ChangedDate = DateTime.UtcNow,
            ChangeSummary = AuditDiffHelpers.Truncate(summary, 2048),
            Name = e.Name,
            SourceModifiedDate = e.ModifiedDate,
        };

    private static string BuildDiffSummary(Snapshot before, UnitMeasure after)
    {
        var sb = new StringBuilder();
        AuditDiffHelpers.AppendIfChanged(sb, "Name", before.Name, after.Name);
        return sb.Length == 0 ? "No changes" : sb.ToString();
    }

    public readonly record struct Snapshot(string Name)
    {
        public Snapshot(UnitMeasure e) : this(e.Name) { }
    }
}
