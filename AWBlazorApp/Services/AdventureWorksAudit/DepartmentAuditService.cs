using System.Text;
using AWBlazorApp.Data.Entities.AdventureWorks;

namespace AWBlazorApp.Services.AdventureWorksAudit;

public static class DepartmentAuditService
{
    public const string ActionCreated = "Created";
    public const string ActionUpdated = "Updated";
    public const string ActionDeleted = "Deleted";

    public static Snapshot CaptureSnapshot(Department e) => new(e);

    public static DepartmentAuditLog RecordCreate(Department e, string? by)
        => BuildLog(e, ActionCreated, by, "Created");

    public static DepartmentAuditLog RecordUpdate(Snapshot before, Department after, string? by)
        => BuildLog(after, ActionUpdated, by, BuildDiffSummary(before, after));

    public static DepartmentAuditLog RecordDelete(Department e, string? by)
        => BuildLog(e, ActionDeleted, by, "Deleted");

    private static DepartmentAuditLog BuildLog(Department e, string action, string? by, string? summary)
        => new()
        {
            DepartmentId = e.Id,
            Action = action,
            ChangedBy = by,
            ChangedDate = DateTime.UtcNow,
            ChangeSummary = AuditDiffHelpers.Truncate(summary, 2048),
            Name = e.Name,
            GroupName = e.GroupName,
            SourceModifiedDate = e.ModifiedDate,
        };

    private static string BuildDiffSummary(Snapshot before, Department after)
    {
        var sb = new StringBuilder();
        AuditDiffHelpers.AppendIfChanged(sb, "Name", before.Name, after.Name);
        AuditDiffHelpers.AppendIfChanged(sb, "GroupName", before.GroupName, after.GroupName);
        return sb.Length == 0 ? "No changes" : sb.ToString();
    }

    public readonly record struct Snapshot(string Name, string GroupName)
    {
        public Snapshot(Department e) : this(e.Name, e.GroupName) { }
    }
}
