using AWBlazorApp.Features.Workforce.Domain;

namespace AWBlazorApp.Features.Workforce.Dtos;

public sealed record ShiftHandoverNoteDto(
    int Id, int StationId, DateOnly ShiftDate, int? FromShiftId, int? ToShiftId,
    string Note, string? AuthoredByUserId, DateTime AuthoredAt,
    bool RequiresAcknowledgment, DateTime? AcknowledgedAt, string? AcknowledgedByUserId, DateTime ModifiedDate);

public sealed record CreateShiftHandoverNoteRequest
{
    public int StationId { get; set; }
    public DateOnly ShiftDate { get; set; }
    public int? FromShiftId { get; set; }
    public int? ToShiftId { get; set; }
    public string? Note { get; set; }
    public bool RequiresAcknowledgment { get; set; }
}

public sealed record AnnouncementDto(
    int Id, string Title, string Body, AnnouncementSeverity Severity,
    int? OrganizationId, int? OrgUnitId,
    DateTime PublishedAt, DateTime? ExpiresAt,
    string? AuthoredByUserId, bool IsActive, DateTime ModifiedDate);

public sealed record CreateAnnouncementRequest
{
    public string? Title { get; set; }
    public string? Body { get; set; }
    public AnnouncementSeverity Severity { get; set; } = AnnouncementSeverity.Info;
    public int? OrganizationId { get; set; }
    public int? OrgUnitId { get; set; }
    public DateTime? ExpiresAt { get; set; }
}

public sealed record UpdateAnnouncementRequest
{
    public string? Title { get; set; }
    public string? Body { get; set; }
    public AnnouncementSeverity? Severity { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool? IsActive { get; set; }
}

public sealed record AnnouncementAuditLogDto(
    int Id, int AnnouncementId, string Action, string? ChangedBy, DateTime ChangedDate, string? ChangeSummary,
    string? Title, AnnouncementSeverity Severity, int? OrganizationId, int? OrgUnitId,
    DateTime PublishedAt, DateTime? ExpiresAt, string? AuthoredByUserId, bool IsActive, DateTime SourceModifiedDate);

public static class CommunicationMappings
{
    public static ShiftHandoverNoteDto ToDto(this ShiftHandoverNote e) => new(
        e.Id, e.StationId, e.ShiftDate, e.FromShiftId, e.ToShiftId,
        e.Note, e.AuthoredByUserId, e.AuthoredAt,
        e.RequiresAcknowledgment, e.AcknowledgedAt, e.AcknowledgedByUserId, e.ModifiedDate);

    public static ShiftHandoverNote ToEntity(this CreateShiftHandoverNoteRequest r, string? userId) => new()
    {
        StationId = r.StationId,
        ShiftDate = r.ShiftDate,
        FromShiftId = r.FromShiftId,
        ToShiftId = r.ToShiftId,
        Note = (r.Note ?? string.Empty).Trim(),
        AuthoredByUserId = userId,
        AuthoredAt = DateTime.UtcNow,
        RequiresAcknowledgment = r.RequiresAcknowledgment,
        ModifiedDate = DateTime.UtcNow,
    };

    public static AnnouncementDto ToDto(this Announcement e) => new(
        e.Id, e.Title, e.Body, e.Severity, e.OrganizationId, e.OrgUnitId,
        e.PublishedAt, e.ExpiresAt, e.AuthoredByUserId, e.IsActive, e.ModifiedDate);

    public static Announcement ToEntity(this CreateAnnouncementRequest r, string? userId)
    {
        var now = DateTime.UtcNow;
        return new Announcement
        {
            Title = (r.Title ?? string.Empty).Trim(),
            Body = r.Body ?? string.Empty,
            Severity = r.Severity,
            OrganizationId = r.OrganizationId,
            OrgUnitId = r.OrgUnitId,
            PublishedAt = now,
            ExpiresAt = r.ExpiresAt,
            AuthoredByUserId = userId,
            IsActive = true,
            ModifiedDate = now,
        };
    }

    public static void ApplyTo(this UpdateAnnouncementRequest r, Announcement e)
    {
        if (r.Title is not null) e.Title = r.Title.Trim();
        if (r.Body is not null) e.Body = r.Body;
        if (r.Severity is not null) e.Severity = r.Severity.Value;
        if (r.ExpiresAt is not null) e.ExpiresAt = r.ExpiresAt;
        if (r.IsActive is not null) e.IsActive = r.IsActive.Value;
        e.ModifiedDate = DateTime.UtcNow;
    }

    public static AnnouncementAuditLogDto ToDto(this AnnouncementAuditLog a) => new(
        a.Id, a.AnnouncementId, a.Action, a.ChangedBy, a.ChangedDate, a.ChangeSummary,
        a.Title, a.Severity, a.OrganizationId, a.OrgUnitId,
        a.PublishedAt, a.ExpiresAt, a.AuthoredByUserId, a.IsActive, a.SourceModifiedDate);
}
