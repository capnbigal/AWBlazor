using AWBlazorApp.Shared.Audit;
using AWBlazorApp.Features.Workforce.Domain;
using System.Text;

namespace AWBlazorApp.Features.Workforce.Audit;

public static class TrainingCourseAuditService
{
    public const string ActionCreated = "Created";
    public const string ActionUpdated = "Updated";
    public const string ActionDeleted = "Deleted";

    public static Snapshot CaptureSnapshot(TrainingCourse e) => new(e);
    public static TrainingCourseAuditLog RecordCreate(TrainingCourse e, string? by) => Build(e, ActionCreated, by, "Created");
    public static TrainingCourseAuditLog RecordUpdate(Snapshot before, TrainingCourse after, string? by) => Build(after, ActionUpdated, by, Diff(before, after));
    public static TrainingCourseAuditLog RecordDelete(TrainingCourse e, string? by) => Build(e, ActionDeleted, by, "Deleted");

    private static TrainingCourseAuditLog Build(TrainingCourse e, string action, string? by, string? summary) => new()
    {
        TrainingCourseId = e.Id,
        Action = action, ChangedBy = by, ChangedDate = DateTime.UtcNow,
        ChangeSummary = AuditDiffHelpers.Truncate(summary, 2048),
        Code = e.Code, Name = e.Name, Description = e.Description,
        DurationMinutes = e.DurationMinutes, RecurrenceMonths = e.RecurrenceMonths,
        IsActive = e.IsActive, SourceModifiedDate = e.ModifiedDate,
    };

    private static string Diff(Snapshot b, TrainingCourse a)
    {
        var sb = new StringBuilder();
        AuditDiffHelpers.AppendIfChanged(sb, "Name", b.Name, a.Name);
        AuditDiffHelpers.AppendIfChanged(sb, "DurationMinutes", b.DurationMinutes, a.DurationMinutes);
        AuditDiffHelpers.AppendIfChanged(sb, "RecurrenceMonths", b.RecurrenceMonths, a.RecurrenceMonths);
        AuditDiffHelpers.AppendIfChanged(sb, "IsActive", b.IsActive, a.IsActive);
        return sb.Length == 0 ? "No changes" : sb.ToString();
    }

    public readonly record struct Snapshot(string Name, int? DurationMinutes, int? RecurrenceMonths, bool IsActive)
    {
        public Snapshot(TrainingCourse e) : this(e.Name, e.DurationMinutes, e.RecurrenceMonths, e.IsActive) { }
    }
}

public static class QualificationAuditService
{
    public const string ActionCreated = "Created";
    public const string ActionUpdated = "Updated";
    public const string ActionDeleted = "Deleted";

    public static Snapshot CaptureSnapshot(Qualification e) => new(e);
    public static QualificationAuditLog RecordCreate(Qualification e, string? by) => Build(e, ActionCreated, by, "Created");
    public static QualificationAuditLog RecordUpdate(Snapshot before, Qualification after, string? by) => Build(after, ActionUpdated, by, Diff(before, after));
    public static QualificationAuditLog RecordDelete(Qualification e, string? by) => Build(e, ActionDeleted, by, "Deleted");

    private static QualificationAuditLog Build(Qualification e, string action, string? by, string? summary) => new()
    {
        QualificationId = e.Id,
        Action = action, ChangedBy = by, ChangedDate = DateTime.UtcNow,
        ChangeSummary = AuditDiffHelpers.Truncate(summary, 2048),
        Code = e.Code, Name = e.Name, Description = e.Description,
        Category = e.Category, IsActive = e.IsActive, SourceModifiedDate = e.ModifiedDate,
    };

    private static string Diff(Snapshot b, Qualification a)
    {
        var sb = new StringBuilder();
        AuditDiffHelpers.AppendIfChanged(sb, "Name", b.Name, a.Name);
        AuditDiffHelpers.AppendIfChanged(sb, "Category", b.Category, a.Category);
        AuditDiffHelpers.AppendIfChanged(sb, "IsActive", b.IsActive, a.IsActive);
        return sb.Length == 0 ? "No changes" : sb.ToString();
    }

    public readonly record struct Snapshot(string Name, QualificationCategory Category, bool IsActive)
    {
        public Snapshot(Qualification e) : this(e.Name, e.Category, e.IsActive) { }
    }
}

public static class EmployeeQualificationAuditService
{
    public const string ActionCreated = "Created";
    public const string ActionUpdated = "Updated";
    public const string ActionDeleted = "Deleted";

    public static Snapshot CaptureSnapshot(EmployeeQualification e) => new(e);
    public static EmployeeQualificationAuditLog RecordCreate(EmployeeQualification e, string? by) => Build(e, ActionCreated, by, "Created");
    public static EmployeeQualificationAuditLog RecordUpdate(Snapshot before, EmployeeQualification after, string? by) => Build(after, ActionUpdated, by, Diff(before, after));
    public static EmployeeQualificationAuditLog RecordDelete(EmployeeQualification e, string? by) => Build(e, ActionDeleted, by, "Revoked");

    private static EmployeeQualificationAuditLog Build(EmployeeQualification e, string action, string? by, string? summary) => new()
    {
        EmployeeQualificationId = e.Id,
        Action = action, ChangedBy = by, ChangedDate = DateTime.UtcNow,
        ChangeSummary = AuditDiffHelpers.Truncate(summary, 2048),
        BusinessEntityId = e.BusinessEntityId, QualificationId = e.QualificationId,
        EarnedDate = e.EarnedDate, ExpiresOn = e.ExpiresOn,
        EvidenceUrl = e.EvidenceUrl, VerifiedByUserId = e.VerifiedByUserId,
        Notes = e.Notes, SourceModifiedDate = e.ModifiedDate,
    };

    private static string Diff(Snapshot b, EmployeeQualification a)
    {
        var sb = new StringBuilder();
        AuditDiffHelpers.AppendIfChanged(sb, "EarnedDate", b.EarnedDate, a.EarnedDate);
        AuditDiffHelpers.AppendIfChanged(sb, "ExpiresOn", b.ExpiresOn, a.ExpiresOn);
        AuditDiffHelpers.AppendIfChanged(sb, "EvidenceUrl", b.EvidenceUrl, a.EvidenceUrl);
        AuditDiffHelpers.AppendIfChanged(sb, "Notes", b.Notes, a.Notes);
        return sb.Length == 0 ? "No changes" : sb.ToString();
    }

    public readonly record struct Snapshot(DateTime EarnedDate, DateTime? ExpiresOn, string? EvidenceUrl, string? Notes)
    {
        public Snapshot(EmployeeQualification e) : this(e.EarnedDate, e.ExpiresOn, e.EvidenceUrl, e.Notes) { }
    }
}

public static class StationQualificationAuditService
{
    public const string ActionCreated = "Created";
    public const string ActionUpdated = "Updated";
    public const string ActionDeleted = "Deleted";

    public static StationQualificationAuditLog RecordCreate(StationQualification e, string? by) => Build(e, ActionCreated, by, "Created");
    public static StationQualificationAuditLog RecordUpdate(StationQualification e, string? by) => Build(e, ActionUpdated, by, "Updated");
    public static StationQualificationAuditLog RecordDelete(StationQualification e, string? by) => Build(e, ActionDeleted, by, "Deleted");

    private static StationQualificationAuditLog Build(StationQualification e, string action, string? by, string? summary) => new()
    {
        StationQualificationId = e.Id,
        Action = action, ChangedBy = by, ChangedDate = DateTime.UtcNow,
        ChangeSummary = AuditDiffHelpers.Truncate(summary, 2048),
        StationId = e.StationId, QualificationId = e.QualificationId,
        IsRequired = e.IsRequired, SourceModifiedDate = e.ModifiedDate,
    };
}
