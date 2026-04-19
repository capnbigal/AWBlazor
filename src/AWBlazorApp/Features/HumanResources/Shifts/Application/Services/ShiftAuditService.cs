using AWBlazorApp.Shared.Audit;
using System.Text;
using AWBlazorApp.Features.HumanResources.Departments.Domain; using AWBlazorApp.Features.HumanResources.Employees.Domain; using AWBlazorApp.Features.HumanResources.EmployeeDepartmentHistories.Domain; using AWBlazorApp.Features.HumanResources.EmployeePayHistories.Domain; using AWBlazorApp.Features.HumanResources.JobCandidates.Domain; using AWBlazorApp.Features.HumanResources.Shifts.Domain; 

namespace AWBlazorApp.Features.HumanResources.Shifts.Application.Services;

public static class ShiftAuditService
{
    public const string ActionCreated = "Created";
    public const string ActionUpdated = "Updated";
    public const string ActionDeleted = "Deleted";

    public static Snapshot CaptureSnapshot(Shift e) => new(e);

    public static ShiftAuditLog RecordCreate(Shift e, string? by)
        => BuildLog(e, ActionCreated, by, "Created");

    public static ShiftAuditLog RecordUpdate(Snapshot before, Shift after, string? by)
        => BuildLog(after, ActionUpdated, by, BuildDiffSummary(before, after));

    public static ShiftAuditLog RecordDelete(Shift e, string? by)
        => BuildLog(e, ActionDeleted, by, "Deleted");

    private static ShiftAuditLog BuildLog(Shift e, string action, string? by, string? summary)
        => new()
        {
            ShiftId = e.Id,
            Action = action,
            ChangedBy = by,
            ChangedDate = DateTime.UtcNow,
            ChangeSummary = AuditDiffHelpers.Truncate(summary, 2048),
            Name = e.Name,
            StartTime = e.StartTime,
            EndTime = e.EndTime,
            SourceModifiedDate = e.ModifiedDate,
        };

    private static string BuildDiffSummary(Snapshot before, Shift after)
    {
        var sb = new StringBuilder();
        AuditDiffHelpers.AppendIfChanged(sb, "Name", before.Name, after.Name);
        AuditDiffHelpers.AppendIfChanged(sb, "StartTime", before.StartTime, after.StartTime);
        AuditDiffHelpers.AppendIfChanged(sb, "EndTime", before.EndTime, after.EndTime);
        return sb.Length == 0 ? "No changes" : sb.ToString();
    }

    public readonly record struct Snapshot(string Name, TimeSpan StartTime, TimeSpan EndTime)
    {
        public Snapshot(Shift e) : this(e.Name, e.StartTime, e.EndTime) { }
    }
}
