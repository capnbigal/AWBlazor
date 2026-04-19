using AWBlazorApp.Shared.Audit;
using System.Text;
using AWBlazorApp.Features.HumanResources.Departments.Domain; using AWBlazorApp.Features.HumanResources.Employees.Domain; using AWBlazorApp.Features.HumanResources.EmployeeDepartmentHistories.Domain; using AWBlazorApp.Features.HumanResources.EmployeePayHistories.Domain; using AWBlazorApp.Features.HumanResources.JobCandidates.Domain; using AWBlazorApp.Features.HumanResources.Shifts.Domain; 

namespace AWBlazorApp.Features.HumanResources.Employees.Application.Services;

public static class EmployeeAuditService
{
    public const string ActionCreated = "Created";
    public const string ActionUpdated = "Updated";
    public const string ActionDeleted = "Deleted";

    public static Snapshot CaptureSnapshot(Employee e) => new(e);

    public static EmployeeAuditLog RecordCreate(Employee e, string? by)
        => BuildLog(e, ActionCreated, by, "Created");

    public static EmployeeAuditLog RecordUpdate(Snapshot before, Employee after, string? by)
        => BuildLog(after, ActionUpdated, by, BuildDiffSummary(before, after));

    public static EmployeeAuditLog RecordDelete(Employee e, string? by)
        => BuildLog(e, ActionDeleted, by, "Deleted");

    private static EmployeeAuditLog BuildLog(Employee e, string action, string? by, string? summary)
        => new()
        {
            EmployeeId = e.Id,
            Action = action,
            ChangedBy = by,
            ChangedDate = DateTime.UtcNow,
            ChangeSummary = AuditDiffHelpers.Truncate(summary, 2048),
            NationalIDNumber = e.NationalIDNumber,
            LoginID = e.LoginID,
            JobTitle = e.JobTitle,
            BirthDate = e.BirthDate,
            MaritalStatus = e.MaritalStatus,
            Gender = e.Gender,
            HireDate = e.HireDate,
            SalariedFlag = e.SalariedFlag,
            CurrentFlag = e.CurrentFlag,
            VacationHours = e.VacationHours,
            SickLeaveHours = e.SickLeaveHours,
            RowGuid = e.RowGuid,
            SourceModifiedDate = e.ModifiedDate,
        };

    private static string BuildDiffSummary(Snapshot before, Employee after)
    {
        var sb = new StringBuilder();
        AuditDiffHelpers.AppendIfChanged(sb, "NationalIDNumber", before.NationalIDNumber, after.NationalIDNumber);
        AuditDiffHelpers.AppendIfChanged(sb, "LoginID", before.LoginID, after.LoginID);
        AuditDiffHelpers.AppendIfChanged(sb, "JobTitle", before.JobTitle, after.JobTitle);
        AuditDiffHelpers.AppendIfChanged(sb, "BirthDate", before.BirthDate, after.BirthDate);
        AuditDiffHelpers.AppendIfChanged(sb, "MaritalStatus", before.MaritalStatus, after.MaritalStatus);
        AuditDiffHelpers.AppendIfChanged(sb, "Gender", before.Gender, after.Gender);
        AuditDiffHelpers.AppendIfChanged(sb, "HireDate", before.HireDate, after.HireDate);
        AuditDiffHelpers.AppendIfChanged(sb, "SalariedFlag", before.SalariedFlag, after.SalariedFlag);
        AuditDiffHelpers.AppendIfChanged(sb, "CurrentFlag", before.CurrentFlag, after.CurrentFlag);
        AuditDiffHelpers.AppendIfChanged(sb, "VacationHours", before.VacationHours, after.VacationHours);
        AuditDiffHelpers.AppendIfChanged(sb, "SickLeaveHours", before.SickLeaveHours, after.SickLeaveHours);
        return sb.Length == 0 ? "No changes" : sb.ToString();
    }

    public readonly record struct Snapshot(
        string NationalIDNumber, string LoginID, string JobTitle,
        DateTime BirthDate, string MaritalStatus, string Gender, DateTime HireDate,
        bool SalariedFlag, bool CurrentFlag, short VacationHours, short SickLeaveHours)
    {
        public Snapshot(Employee e) : this(
            e.NationalIDNumber, e.LoginID, e.JobTitle,
            e.BirthDate, e.MaritalStatus, e.Gender, e.HireDate,
            e.SalariedFlag, e.CurrentFlag, e.VacationHours, e.SickLeaveHours)
        { }
    }
}
