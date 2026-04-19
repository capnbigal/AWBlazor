using AWBlazorApp.Features.Maintenance.AssetProfiles.Domain; using AWBlazorApp.Features.Maintenance.Logs.Domain; using AWBlazorApp.Features.Maintenance.MeterReadings.Domain; using AWBlazorApp.Features.Maintenance.PmSchedules.Domain; using AWBlazorApp.Features.Maintenance.SpareParts.Domain; using AWBlazorApp.Features.Maintenance.WorkOrders.Domain; 

namespace AWBlazorApp.Features.Maintenance.PmSchedules.Dtos;

public sealed record PmScheduleDto(
    int Id, string Code, string Name, string? Description,
    int AssetId, PmIntervalKind IntervalKind, int IntervalValue,
    WorkOrderPriority DefaultPriority, int EstimatedMinutes, bool IsActive,
    DateTime? LastCompletedAt, decimal? LastCompletedMeterValue, DateTime ModifiedDate);

public sealed record CreatePmScheduleRequest
{
    public string? Code { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public int AssetId { get; set; }
    public PmIntervalKind IntervalKind { get; set; } = PmIntervalKind.Days;
    public int IntervalValue { get; set; }
    public WorkOrderPriority DefaultPriority { get; set; } = WorkOrderPriority.Medium;
    public int EstimatedMinutes { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed record UpdatePmScheduleRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public PmIntervalKind? IntervalKind { get; set; }
    public int? IntervalValue { get; set; }
    public WorkOrderPriority? DefaultPriority { get; set; }
    public int? EstimatedMinutes { get; set; }
    public bool? IsActive { get; set; }
}

public sealed record PmScheduleAuditLogDto(
    int Id, int PmScheduleId, string Action, string? ChangedBy, DateTime ChangedDate, string? ChangeSummary,
    string? Code, string? Name, string? Description,
    int AssetId, PmIntervalKind IntervalKind, int IntervalValue,
    WorkOrderPriority DefaultPriority, int EstimatedMinutes, bool IsActive,
    DateTime? LastCompletedAt, decimal? LastCompletedMeterValue, DateTime SourceModifiedDate);

public sealed record PmScheduleTaskDto(
    int Id, int PmScheduleId, int SequenceNumber, string TaskName,
    string? Instructions, int? EstimatedMinutes, bool RequiresSignoff, DateTime ModifiedDate);

public sealed record CreatePmScheduleTaskRequest
{
    public int PmScheduleId { get; set; }
    public int SequenceNumber { get; set; }
    public string? TaskName { get; set; }
    public string? Instructions { get; set; }
    public int? EstimatedMinutes { get; set; }
    public bool RequiresSignoff { get; set; }
}

public sealed record GenerateDueWorkOrdersResponse(int Generated);

public static class PmScheduleMappings
{
    public static PmScheduleDto ToDto(this PmSchedule e) => new(
        e.Id, e.Code, e.Name, e.Description,
        e.AssetId, e.IntervalKind, e.IntervalValue,
        e.DefaultPriority, e.EstimatedMinutes, e.IsActive,
        e.LastCompletedAt, e.LastCompletedMeterValue, e.ModifiedDate);

    public static PmSchedule ToEntity(this CreatePmScheduleRequest r) => new()
    {
        Code = (r.Code ?? string.Empty).Trim().ToUpperInvariant(),
        Name = (r.Name ?? string.Empty).Trim(),
        Description = r.Description?.Trim(),
        AssetId = r.AssetId,
        IntervalKind = r.IntervalKind,
        IntervalValue = r.IntervalValue,
        DefaultPriority = r.DefaultPriority,
        EstimatedMinutes = r.EstimatedMinutes,
        IsActive = r.IsActive,
        ModifiedDate = DateTime.UtcNow,
    };

    public static void ApplyTo(this UpdatePmScheduleRequest r, PmSchedule e)
    {
        if (r.Name is not null) e.Name = r.Name.Trim();
        if (r.Description is not null) e.Description = r.Description.Trim();
        if (r.IntervalKind is not null) e.IntervalKind = r.IntervalKind.Value;
        if (r.IntervalValue is not null) e.IntervalValue = r.IntervalValue.Value;
        if (r.DefaultPriority is not null) e.DefaultPriority = r.DefaultPriority.Value;
        if (r.EstimatedMinutes is not null) e.EstimatedMinutes = r.EstimatedMinutes.Value;
        if (r.IsActive is not null) e.IsActive = r.IsActive.Value;
        e.ModifiedDate = DateTime.UtcNow;
    }

    public static PmScheduleAuditLogDto ToDto(this PmScheduleAuditLog a) => new(
        a.Id, a.PmScheduleId, a.Action, a.ChangedBy, a.ChangedDate, a.ChangeSummary,
        a.Code, a.Name, a.Description,
        a.AssetId, a.IntervalKind, a.IntervalValue,
        a.DefaultPriority, a.EstimatedMinutes, a.IsActive,
        a.LastCompletedAt, a.LastCompletedMeterValue, a.SourceModifiedDate);

    public static PmScheduleTaskDto ToDto(this PmScheduleTask e) => new(
        e.Id, e.PmScheduleId, e.SequenceNumber, e.TaskName,
        e.Instructions, e.EstimatedMinutes, e.RequiresSignoff, e.ModifiedDate);

    public static PmScheduleTask ToEntity(this CreatePmScheduleTaskRequest r) => new()
    {
        PmScheduleId = r.PmScheduleId,
        SequenceNumber = r.SequenceNumber,
        TaskName = (r.TaskName ?? string.Empty).Trim(),
        Instructions = r.Instructions?.Trim(),
        EstimatedMinutes = r.EstimatedMinutes,
        RequiresSignoff = r.RequiresSignoff,
        ModifiedDate = DateTime.UtcNow,
    };
}
