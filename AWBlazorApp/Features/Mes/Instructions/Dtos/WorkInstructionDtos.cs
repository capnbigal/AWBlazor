using AWBlazorApp.Features.Mes.Downtime.Domain; using AWBlazorApp.Features.Mes.Instructions.Domain; using AWBlazorApp.Features.Mes.Runs.Domain; 

namespace AWBlazorApp.Features.Mes.Instructions.Dtos;

public sealed record WorkInstructionDto(
    int Id, int WorkOrderRoutingId, string Title, int? ActiveRevisionId, bool IsActive, DateTime ModifiedDate);

public sealed record CreateWorkInstructionRequest
{
    public int WorkOrderRoutingId { get; set; }
    public string? Title { get; set; }
}

public sealed record UpdateWorkInstructionRequest
{
    public string? Title { get; set; }
    public bool? IsActive { get; set; }
}

public sealed record WorkInstructionAuditLogDto(
    int Id, int WorkInstructionId, string Action, string? ChangedBy, DateTime ChangedDate, string? ChangeSummary,
    int WorkOrderRoutingId, string? Title, int? ActiveRevisionId, bool IsActive, DateTime SourceModifiedDate);

public sealed record WorkInstructionRevisionDto(
    int Id, int WorkInstructionId, int RevisionNumber, WorkInstructionRevisionStatus Status,
    string? CreatedByUserId, DateTime CreatedDate, DateTime? PublishedAt, string? Notes, DateTime ModifiedDate);

public sealed record WorkInstructionRevisionAuditLogDto(
    int Id, int WorkInstructionRevisionId, string Action, string? ChangedBy, DateTime ChangedDate, string? ChangeSummary,
    int WorkInstructionId, int RevisionNumber, WorkInstructionRevisionStatus Status,
    string? CreatedByUserId, DateTime CreatedDate, DateTime? PublishedAt, string? Notes, DateTime SourceModifiedDate);

public sealed record WorkInstructionStepDto(
    int Id, int WorkInstructionRevisionId, int SequenceNumber, string Title, string Body,
    string? AttachmentUrl, int? EstimatedDurationMinutes, DateTime ModifiedDate);

public sealed record CreateWorkInstructionStepRequest
{
    public int WorkInstructionRevisionId { get; set; }
    public int SequenceNumber { get; set; }
    public string? Title { get; set; }
    public string? Body { get; set; }
    public string? AttachmentUrl { get; set; }
    public int? EstimatedDurationMinutes { get; set; }
}

public sealed record UpdateWorkInstructionStepRequest
{
    public int? SequenceNumber { get; set; }
    public string? Title { get; set; }
    public string? Body { get; set; }
    public string? AttachmentUrl { get; set; }
    public int? EstimatedDurationMinutes { get; set; }
}

public sealed record WorkInstructionStepAuditLogDto(
    int Id, int WorkInstructionStepId, string Action, string? ChangedBy, DateTime ChangedDate, string? ChangeSummary,
    int WorkInstructionRevisionId, int SequenceNumber, string? Title,
    string? AttachmentUrl, int? EstimatedDurationMinutes, DateTime SourceModifiedDate);

public static class WorkInstructionMappings
{
    public static WorkInstructionDto ToDto(this WorkInstruction e) => new(
        e.Id, e.WorkOrderRoutingId, e.Title, e.ActiveRevisionId, e.IsActive, e.ModifiedDate);

    public static WorkInstruction ToEntity(this CreateWorkInstructionRequest r) => new()
    {
        WorkOrderRoutingId = r.WorkOrderRoutingId,
        Title = (r.Title ?? string.Empty).Trim(),
        IsActive = true,
        ModifiedDate = DateTime.UtcNow,
    };

    public static void ApplyTo(this UpdateWorkInstructionRequest r, WorkInstruction e)
    {
        if (r.Title is not null) e.Title = r.Title.Trim();
        if (r.IsActive is not null) e.IsActive = r.IsActive.Value;
        e.ModifiedDate = DateTime.UtcNow;
    }

    public static WorkInstructionAuditLogDto ToDto(this WorkInstructionAuditLog a) => new(
        a.Id, a.WorkInstructionId, a.Action, a.ChangedBy, a.ChangedDate, a.ChangeSummary,
        a.WorkOrderRoutingId, a.Title, a.ActiveRevisionId, a.IsActive, a.SourceModifiedDate);

    public static WorkInstructionRevisionDto ToDto(this WorkInstructionRevision e) => new(
        e.Id, e.WorkInstructionId, e.RevisionNumber, e.Status, e.CreatedByUserId,
        e.CreatedDate, e.PublishedAt, e.Notes, e.ModifiedDate);

    public static WorkInstructionRevisionAuditLogDto ToDto(this WorkInstructionRevisionAuditLog a) => new(
        a.Id, a.WorkInstructionRevisionId, a.Action, a.ChangedBy, a.ChangedDate, a.ChangeSummary,
        a.WorkInstructionId, a.RevisionNumber, a.Status, a.CreatedByUserId,
        a.CreatedDate, a.PublishedAt, a.Notes, a.SourceModifiedDate);

    public static WorkInstructionStepDto ToDto(this WorkInstructionStep e) => new(
        e.Id, e.WorkInstructionRevisionId, e.SequenceNumber, e.Title, e.Body,
        e.AttachmentUrl, e.EstimatedDurationMinutes, e.ModifiedDate);

    public static WorkInstructionStep ToEntity(this CreateWorkInstructionStepRequest r) => new()
    {
        WorkInstructionRevisionId = r.WorkInstructionRevisionId,
        SequenceNumber = r.SequenceNumber,
        Title = (r.Title ?? string.Empty).Trim(),
        Body = r.Body ?? string.Empty,
        AttachmentUrl = r.AttachmentUrl?.Trim(),
        EstimatedDurationMinutes = r.EstimatedDurationMinutes,
        ModifiedDate = DateTime.UtcNow,
    };

    public static void ApplyTo(this UpdateWorkInstructionStepRequest r, WorkInstructionStep e)
    {
        if (r.SequenceNumber is not null) e.SequenceNumber = r.SequenceNumber.Value;
        if (r.Title is not null) e.Title = r.Title.Trim();
        if (r.Body is not null) e.Body = r.Body;
        if (r.AttachmentUrl is not null) e.AttachmentUrl = r.AttachmentUrl.Trim();
        if (r.EstimatedDurationMinutes is not null) e.EstimatedDurationMinutes = r.EstimatedDurationMinutes;
        e.ModifiedDate = DateTime.UtcNow;
    }

    public static WorkInstructionStepAuditLogDto ToDto(this WorkInstructionStepAuditLog a) => new(
        a.Id, a.WorkInstructionStepId, a.Action, a.ChangedBy, a.ChangedDate, a.ChangeSummary,
        a.WorkInstructionRevisionId, a.SequenceNumber, a.Title,
        a.AttachmentUrl, a.EstimatedDurationMinutes, a.SourceModifiedDate);
}
