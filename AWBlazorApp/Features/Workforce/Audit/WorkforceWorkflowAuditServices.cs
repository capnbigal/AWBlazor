using AWBlazorApp.Features.AdventureWorks.Audit;
using AWBlazorApp.Features.Workforce.Domain;
using System.Text;

namespace AWBlazorApp.Features.Workforce.Audit;

public static class QualificationAlertAuditService
{
    public const string ActionCreated = "Created";
    public const string ActionUpdated = "Updated";
    public const string ActionDeleted = "Deleted";

    public static Snapshot CaptureSnapshot(QualificationAlert e) => new(e);
    public static QualificationAlertAuditLog RecordCreate(QualificationAlert e, string? by) => Build(e, ActionCreated, by, "Raised");
    public static QualificationAlertAuditLog RecordUpdate(Snapshot before, QualificationAlert after, string? by) => Build(after, ActionUpdated, by, Diff(before, after));

    private static QualificationAlertAuditLog Build(QualificationAlert e, string action, string? by, string? summary) => new()
    {
        QualificationAlertId = e.Id,
        Action = action, ChangedBy = by, ChangedDate = DateTime.UtcNow,
        ChangeSummary = AuditDiffHelpers.Truncate(summary, 2048),
        BusinessEntityId = e.BusinessEntityId, StationId = e.StationId, QualificationId = e.QualificationId,
        OperatorClockEventId = e.OperatorClockEventId, Reason = e.Reason, Status = e.Status,
        RaisedAt = e.RaisedAt, AcknowledgedAt = e.AcknowledgedAt, AcknowledgedByUserId = e.AcknowledgedByUserId,
        Notes = e.Notes, SourceModifiedDate = e.ModifiedDate,
    };

    private static string Diff(Snapshot b, QualificationAlert a)
    {
        var sb = new StringBuilder();
        AuditDiffHelpers.AppendIfChanged(sb, "Status", b.Status, a.Status);
        AuditDiffHelpers.AppendIfChanged(sb, "AcknowledgedByUserId", b.AcknowledgedByUserId, a.AcknowledgedByUserId);
        AuditDiffHelpers.AppendIfChanged(sb, "Notes", b.Notes, a.Notes);
        return sb.Length == 0 ? "No changes" : sb.ToString();
    }

    public readonly record struct Snapshot(QualificationAlertStatus Status, string? AcknowledgedByUserId, string? Notes)
    {
        public Snapshot(QualificationAlert e) : this(e.Status, e.AcknowledgedByUserId, e.Notes) { }
    }
}

public static class LeaveRequestAuditService
{
    public const string ActionCreated = "Created";
    public const string ActionUpdated = "Updated";
    public const string ActionDeleted = "Deleted";

    public static Snapshot CaptureSnapshot(LeaveRequest e) => new(e);
    public static LeaveRequestAuditLog RecordCreate(LeaveRequest e, string? by) => Build(e, ActionCreated, by, "Submitted");
    public static LeaveRequestAuditLog RecordUpdate(Snapshot before, LeaveRequest after, string? by) => Build(after, ActionUpdated, by, Diff(before, after));
    public static LeaveRequestAuditLog RecordDelete(LeaveRequest e, string? by) => Build(e, ActionDeleted, by, "Deleted");

    private static LeaveRequestAuditLog Build(LeaveRequest e, string action, string? by, string? summary) => new()
    {
        LeaveRequestId = e.Id,
        Action = action, ChangedBy = by, ChangedDate = DateTime.UtcNow,
        ChangeSummary = AuditDiffHelpers.Truncate(summary, 2048),
        BusinessEntityId = e.BusinessEntityId, LeaveType = e.LeaveType,
        StartDate = e.StartDate, EndDate = e.EndDate, Status = e.Status,
        Reason = e.Reason, RequestedByUserId = e.RequestedByUserId, RequestedAt = e.RequestedAt,
        ReviewedByUserId = e.ReviewedByUserId, ReviewedAt = e.ReviewedAt, ReviewNotes = e.ReviewNotes,
        SourceModifiedDate = e.ModifiedDate,
    };

    private static string Diff(Snapshot b, LeaveRequest a)
    {
        var sb = new StringBuilder();
        AuditDiffHelpers.AppendIfChanged(sb, "Status", b.Status, a.Status);
        AuditDiffHelpers.AppendIfChanged(sb, "StartDate", b.StartDate, a.StartDate);
        AuditDiffHelpers.AppendIfChanged(sb, "EndDate", b.EndDate, a.EndDate);
        AuditDiffHelpers.AppendIfChanged(sb, "ReviewNotes", b.ReviewNotes, a.ReviewNotes);
        return sb.Length == 0 ? "No changes" : sb.ToString();
    }

    public readonly record struct Snapshot(LeaveStatus Status, DateOnly StartDate, DateOnly EndDate, string? ReviewNotes)
    {
        public Snapshot(LeaveRequest e) : this(e.Status, e.StartDate, e.EndDate, e.ReviewNotes) { }
    }
}

public static class AnnouncementAuditService
{
    public const string ActionCreated = "Created";
    public const string ActionUpdated = "Updated";
    public const string ActionDeleted = "Deleted";

    public static Snapshot CaptureSnapshot(Announcement e) => new(e);
    public static AnnouncementAuditLog RecordCreate(Announcement e, string? by) => Build(e, ActionCreated, by, "Created");
    public static AnnouncementAuditLog RecordUpdate(Snapshot before, Announcement after, string? by) => Build(after, ActionUpdated, by, Diff(before, after));
    public static AnnouncementAuditLog RecordDelete(Announcement e, string? by) => Build(e, ActionDeleted, by, "Deleted");

    private static AnnouncementAuditLog Build(Announcement e, string action, string? by, string? summary) => new()
    {
        AnnouncementId = e.Id,
        Action = action, ChangedBy = by, ChangedDate = DateTime.UtcNow,
        ChangeSummary = AuditDiffHelpers.Truncate(summary, 2048),
        Title = e.Title, Severity = e.Severity,
        OrganizationId = e.OrganizationId, OrgUnitId = e.OrgUnitId,
        PublishedAt = e.PublishedAt, ExpiresAt = e.ExpiresAt,
        AuthoredByUserId = e.AuthoredByUserId, IsActive = e.IsActive,
        SourceModifiedDate = e.ModifiedDate,
    };

    private static string Diff(Snapshot b, Announcement a)
    {
        var sb = new StringBuilder();
        AuditDiffHelpers.AppendIfChanged(sb, "Title", b.Title, a.Title);
        AuditDiffHelpers.AppendIfChanged(sb, "Severity", b.Severity, a.Severity);
        AuditDiffHelpers.AppendIfChanged(sb, "ExpiresAt", b.ExpiresAt, a.ExpiresAt);
        AuditDiffHelpers.AppendIfChanged(sb, "IsActive", b.IsActive, a.IsActive);
        return sb.Length == 0 ? "No changes" : sb.ToString();
    }

    public readonly record struct Snapshot(string Title, AnnouncementSeverity Severity, DateTime? ExpiresAt, bool IsActive)
    {
        public Snapshot(Announcement e) : this(e.Title, e.Severity, e.ExpiresAt, e.IsActive) { }
    }
}
