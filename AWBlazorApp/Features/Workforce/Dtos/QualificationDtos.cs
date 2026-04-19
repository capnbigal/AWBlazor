using AWBlazorApp.Features.Workforce.Domain;

namespace AWBlazorApp.Features.Workforce.Dtos;

public sealed record QualificationDto(
    int Id, string Code, string Name, string? Description, QualificationCategory Category,
    bool IsActive, DateTime ModifiedDate);

public sealed record CreateQualificationRequest
{
    public string? Code { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public QualificationCategory Category { get; set; } = QualificationCategory.Skill;
    public bool IsActive { get; set; } = true;
}

public sealed record UpdateQualificationRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public QualificationCategory? Category { get; set; }
    public bool? IsActive { get; set; }
}

public sealed record QualificationAuditLogDto(
    int Id, int QualificationId, string Action, string? ChangedBy, DateTime ChangedDate, string? ChangeSummary,
    string? Code, string? Name, string? Description, QualificationCategory Category,
    bool IsActive, DateTime SourceModifiedDate);

public sealed record EmployeeQualificationDto(
    int Id, int BusinessEntityId, int QualificationId,
    DateTime EarnedDate, DateTime? ExpiresOn,
    string? EvidenceUrl, string? VerifiedByUserId, string? Notes, DateTime ModifiedDate);

public sealed record GrantEmployeeQualificationRequest
{
    public int BusinessEntityId { get; set; }
    public int QualificationId { get; set; }
    public DateTime? EarnedDate { get; set; }
    public DateTime? ExpiresOn { get; set; }
    public string? EvidenceUrl { get; set; }
    public string? Notes { get; set; }
}

public sealed record EmployeeQualificationAuditLogDto(
    int Id, int EmployeeQualificationId, string Action, string? ChangedBy, DateTime ChangedDate, string? ChangeSummary,
    int BusinessEntityId, int QualificationId, DateTime EarnedDate, DateTime? ExpiresOn,
    string? EvidenceUrl, string? VerifiedByUserId, string? Notes, DateTime SourceModifiedDate);

public sealed record StationQualificationDto(
    int Id, int StationId, int QualificationId, bool IsRequired, DateTime ModifiedDate);

public sealed record CreateStationQualificationRequest
{
    public int StationId { get; set; }
    public int QualificationId { get; set; }
    public bool IsRequired { get; set; } = true;
}

public sealed record UpdateStationQualificationRequest { public bool? IsRequired { get; set; } }

public sealed record QualificationAlertDto(
    long Id, int BusinessEntityId, int StationId, int QualificationId,
    long? OperatorClockEventId, QualificationAlertReason Reason, QualificationAlertStatus Status,
    DateTime RaisedAt, DateTime? AcknowledgedAt, string? AcknowledgedByUserId, string? Notes, DateTime ModifiedDate);

public sealed record AcknowledgeQualificationAlertRequest
{
    public QualificationAlertStatus TargetStatus { get; set; } = QualificationAlertStatus.Acknowledged;
    public string? Notes { get; set; }
}

public static class QualificationMappings
{
    public static QualificationDto ToDto(this Qualification e) => new(
        e.Id, e.Code, e.Name, e.Description, e.Category, e.IsActive, e.ModifiedDate);

    public static Qualification ToEntity(this CreateQualificationRequest r) => new()
    {
        Code = (r.Code ?? string.Empty).Trim().ToUpperInvariant(),
        Name = (r.Name ?? string.Empty).Trim(),
        Description = r.Description?.Trim(),
        Category = r.Category, IsActive = r.IsActive,
        ModifiedDate = DateTime.UtcNow,
    };

    public static void ApplyTo(this UpdateQualificationRequest r, Qualification e)
    {
        if (r.Name is not null) e.Name = r.Name.Trim();
        if (r.Description is not null) e.Description = r.Description.Trim();
        if (r.Category is not null) e.Category = r.Category.Value;
        if (r.IsActive is not null) e.IsActive = r.IsActive.Value;
        e.ModifiedDate = DateTime.UtcNow;
    }

    public static QualificationAuditLogDto ToDto(this QualificationAuditLog a) => new(
        a.Id, a.QualificationId, a.Action, a.ChangedBy, a.ChangedDate, a.ChangeSummary,
        a.Code, a.Name, a.Description, a.Category, a.IsActive, a.SourceModifiedDate);

    public static EmployeeQualificationDto ToDto(this EmployeeQualification e) => new(
        e.Id, e.BusinessEntityId, e.QualificationId,
        e.EarnedDate, e.ExpiresOn, e.EvidenceUrl, e.VerifiedByUserId, e.Notes, e.ModifiedDate);

    public static EmployeeQualificationAuditLogDto ToDto(this EmployeeQualificationAuditLog a) => new(
        a.Id, a.EmployeeQualificationId, a.Action, a.ChangedBy, a.ChangedDate, a.ChangeSummary,
        a.BusinessEntityId, a.QualificationId, a.EarnedDate, a.ExpiresOn,
        a.EvidenceUrl, a.VerifiedByUserId, a.Notes, a.SourceModifiedDate);

    public static StationQualificationDto ToDto(this StationQualification e) => new(
        e.Id, e.StationId, e.QualificationId, e.IsRequired, e.ModifiedDate);

    public static StationQualification ToEntity(this CreateStationQualificationRequest r) => new()
    {
        StationId = r.StationId, QualificationId = r.QualificationId,
        IsRequired = r.IsRequired, ModifiedDate = DateTime.UtcNow,
    };

    public static void ApplyTo(this UpdateStationQualificationRequest r, StationQualification e)
    {
        if (r.IsRequired is not null) e.IsRequired = r.IsRequired.Value;
        e.ModifiedDate = DateTime.UtcNow;
    }

    public static QualificationAlertDto ToDto(this QualificationAlert e) => new(
        e.Id, e.BusinessEntityId, e.StationId, e.QualificationId,
        e.OperatorClockEventId, e.Reason, e.Status,
        e.RaisedAt, e.AcknowledgedAt, e.AcknowledgedByUserId, e.Notes, e.ModifiedDate);
}
