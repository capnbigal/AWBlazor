using AWBlazorApp.Features.Workforce.Domain;

namespace AWBlazorApp.Features.Workforce.Dtos;

public sealed record TrainingCourseDto(
    int Id, string Code, string Name, string? Description,
    int? DurationMinutes, int? RecurrenceMonths, bool IsActive, DateTime ModifiedDate);

public sealed record CreateTrainingCourseRequest
{
    public string? Code { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public int? DurationMinutes { get; set; }
    public int? RecurrenceMonths { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed record UpdateTrainingCourseRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public int? DurationMinutes { get; set; }
    public int? RecurrenceMonths { get; set; }
    public bool? IsActive { get; set; }
}

public sealed record TrainingCourseAuditLogDto(
    int Id, int TrainingCourseId, string Action, string? ChangedBy, DateTime ChangedDate, string? ChangeSummary,
    string? Code, string? Name, string? Description,
    int? DurationMinutes, int? RecurrenceMonths, bool IsActive, DateTime SourceModifiedDate);

public sealed record TrainingRecordDto(
    int Id, int TrainingCourseId, int BusinessEntityId,
    DateTime CompletedAt, DateTime? ExpiresOn,
    string? Score, string? EvidenceUrl, string? Notes,
    string? RecordedByUserId, DateTime ModifiedDate);

public sealed record CreateTrainingRecordRequest
{
    public int TrainingCourseId { get; set; }
    public int BusinessEntityId { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? Score { get; set; }
    public string? EvidenceUrl { get; set; }
    public string? Notes { get; set; }
}

public static class TrainingMappings
{
    public static TrainingCourseDto ToDto(this TrainingCourse e) => new(
        e.Id, e.Code, e.Name, e.Description, e.DurationMinutes, e.RecurrenceMonths, e.IsActive, e.ModifiedDate);

    public static TrainingCourse ToEntity(this CreateTrainingCourseRequest r) => new()
    {
        Code = (r.Code ?? string.Empty).Trim().ToUpperInvariant(),
        Name = (r.Name ?? string.Empty).Trim(),
        Description = r.Description?.Trim(),
        DurationMinutes = r.DurationMinutes,
        RecurrenceMonths = r.RecurrenceMonths,
        IsActive = r.IsActive,
        ModifiedDate = DateTime.UtcNow,
    };

    public static void ApplyTo(this UpdateTrainingCourseRequest r, TrainingCourse e)
    {
        if (r.Name is not null) e.Name = r.Name.Trim();
        if (r.Description is not null) e.Description = r.Description.Trim();
        if (r.DurationMinutes is not null) e.DurationMinutes = r.DurationMinutes;
        if (r.RecurrenceMonths is not null) e.RecurrenceMonths = r.RecurrenceMonths;
        if (r.IsActive is not null) e.IsActive = r.IsActive.Value;
        e.ModifiedDate = DateTime.UtcNow;
    }

    public static TrainingCourseAuditLogDto ToDto(this TrainingCourseAuditLog a) => new(
        a.Id, a.TrainingCourseId, a.Action, a.ChangedBy, a.ChangedDate, a.ChangeSummary,
        a.Code, a.Name, a.Description, a.DurationMinutes, a.RecurrenceMonths, a.IsActive, a.SourceModifiedDate);

    public static TrainingRecordDto ToDto(this TrainingRecord e) => new(
        e.Id, e.TrainingCourseId, e.BusinessEntityId,
        e.CompletedAt, e.ExpiresOn, e.Score, e.EvidenceUrl, e.Notes,
        e.RecordedByUserId, e.ModifiedDate);
}
