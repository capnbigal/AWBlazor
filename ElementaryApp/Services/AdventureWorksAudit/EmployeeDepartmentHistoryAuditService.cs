using System.Text;
using ElementaryApp.Data.Entities.AdventureWorks;

namespace ElementaryApp.Services.AdventureWorksAudit;

public static class EmployeeDepartmentHistoryAuditService
{
    public const string ActionCreated = "Created";
    public const string ActionUpdated = "Updated";
    public const string ActionDeleted = "Deleted";

    public static Snapshot CaptureSnapshot(EmployeeDepartmentHistory e) => new(e);

    public static EmployeeDepartmentHistoryAuditLog RecordCreate(EmployeeDepartmentHistory e, string? by)
        => BuildLog(e, ActionCreated, by, "Created");

    public static EmployeeDepartmentHistoryAuditLog RecordUpdate(Snapshot before, EmployeeDepartmentHistory after, string? by)
        => BuildLog(after, ActionUpdated, by, BuildDiffSummary(before, after));

    public static EmployeeDepartmentHistoryAuditLog RecordDelete(EmployeeDepartmentHistory e, string? by)
        => BuildLog(e, ActionDeleted, by, "Deleted");

    private static EmployeeDepartmentHistoryAuditLog BuildLog(
        EmployeeDepartmentHistory e, string action, string? by, string? summary)
        => new()
        {
            BusinessEntityId = e.BusinessEntityId,
            DepartmentId = e.DepartmentId,
            ShiftId = e.ShiftId,
            StartDate = e.StartDate,
            Action = action,
            ChangedBy = by,
            ChangedDate = DateTime.UtcNow,
            ChangeSummary = AuditDiffHelpers.Truncate(summary, 2048),
            EndDate = e.EndDate,
            SourceModifiedDate = e.ModifiedDate,
        };

    private static string BuildDiffSummary(Snapshot before, EmployeeDepartmentHistory after)
    {
        var sb = new StringBuilder();
        AuditDiffHelpers.AppendIfChanged(sb, "EndDate", before.EndDate, after.EndDate);
        return sb.Length == 0 ? "No changes" : sb.ToString();
    }

    public readonly record struct Snapshot(DateTime? EndDate)
    {
        public Snapshot(EmployeeDepartmentHistory e) : this(e.EndDate) { }
    }
}
