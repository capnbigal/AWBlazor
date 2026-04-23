using AWBlazorApp.Features.Maintenance.AssetProfiles.Domain; using AWBlazorApp.Features.Maintenance.Logs.Domain; using AWBlazorApp.Features.Maintenance.MeterReadings.Domain; using AWBlazorApp.Features.Maintenance.PmSchedules.Domain; using AWBlazorApp.Features.Maintenance.SpareParts.Domain; using AWBlazorApp.Features.Maintenance.WorkOrders.Domain; 

namespace AWBlazorApp.Features.Maintenance.WorkOrders.Dtos;

public sealed record MaintenanceWorkOrderDto(
    int Id, string WorkOrderNumber, string Title, string? Description,
    int AssetId, WorkOrderType Type, WorkOrderStatus Status, WorkOrderPriority Priority,
    int? PmScheduleId, DateTime? ScheduledFor, int? AssignedBusinessEntityId,
    DateTime? StartedAt, DateTime? CompletedAt, DateTime? HeldAt, DateTime? CancelledAt,
    string? CompletionNotes, string? RaisedByUserId, DateTime RaisedAt,
    decimal? CompletedMeterValue, DateTime ModifiedDate);

public sealed record CreateMaintenanceWorkOrderRequest
{
    public string? WorkOrderNumber { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public int AssetId { get; set; }
    public WorkOrderType Type { get; set; } = WorkOrderType.Corrective;
    public WorkOrderPriority Priority { get; set; } = WorkOrderPriority.Medium;
    public DateTime? ScheduledFor { get; set; }
    public int? AssignedBusinessEntityId { get; set; }
}

public sealed record UpdateMaintenanceWorkOrderRequest
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public WorkOrderPriority? Priority { get; set; }
    public int? AssignedBusinessEntityId { get; set; }
}

public sealed record ScheduleWorkOrderRequest
{
    public DateTime ScheduledFor { get; set; }
    public int? AssigneeBusinessEntityId { get; set; }
}

public sealed record CompleteWorkOrderRequest
{
    public string? CompletionNotes { get; set; }
    public decimal? CompletedMeterValue { get; set; }
}

public sealed record WorkOrderStateChangeRequest { public string? Reason { get; set; } }

public sealed record MaintenanceWorkOrderTaskDto(
    int Id, int MaintenanceWorkOrderId, int SequenceNumber, string TaskName,
    string? Instructions, int? EstimatedMinutes, int? ActualMinutes,
    bool RequiresSignoff, bool IsComplete, DateTime? CompletedAt,
    string? CompletedByUserId, string? SignoffNotes, DateTime ModifiedDate);

public sealed record CreateWorkOrderTaskRequest
{
    public int MaintenanceWorkOrderId { get; set; }
    public int SequenceNumber { get; set; }
    public string? TaskName { get; set; }
    public string? Instructions { get; set; }
    public int? EstimatedMinutes { get; set; }
    public bool RequiresSignoff { get; set; }
}

public sealed record CompleteWorkOrderTaskRequest
{
    public int? ActualMinutes { get; set; }
    public string? SignoffNotes { get; set; }
}

public static class WorkOrderMappings
{
    public static MaintenanceWorkOrderDto ToDto(this MaintenanceWorkOrder e) => new(
        e.Id, e.WorkOrderNumber, e.Title, e.Description,
        e.AssetId, e.Type, e.Status, e.Priority,
        e.PmScheduleId, e.ScheduledFor, e.AssignedBusinessEntityId,
        e.StartedAt, e.CompletedAt, e.HeldAt, e.CancelledAt,
        e.CompletionNotes, e.RaisedByUserId, e.RaisedAt,
        e.CompletedMeterValue, e.ModifiedDate);

    public static MaintenanceWorkOrder ToEntity(this CreateMaintenanceWorkOrderRequest r, string? userId)
    {
        var now = DateTime.UtcNow;
        return new MaintenanceWorkOrder
        {
            WorkOrderNumber = (r.WorkOrderNumber ?? string.Empty).Trim().ToUpperInvariant(),
            Title = (r.Title ?? string.Empty).Trim(),
            Description = r.Description?.Trim(),
            AssetId = r.AssetId,
            Type = r.Type,
            Status = WorkOrderStatus.Draft,
            Priority = r.Priority,
            ScheduledFor = r.ScheduledFor,
            AssignedBusinessEntityId = r.AssignedBusinessEntityId,
            RaisedByUserId = userId,
            RaisedAt = now,
            ModifiedDate = now,
        };
    }

    public static void ApplyTo(this UpdateMaintenanceWorkOrderRequest r, MaintenanceWorkOrder e)
    {
        if (r.Title is not null) e.Title = r.Title.Trim();
        if (r.Description is not null) e.Description = r.Description.Trim();
        if (r.Priority is not null) e.Priority = r.Priority.Value;
        if (r.AssignedBusinessEntityId is not null) e.AssignedBusinessEntityId = r.AssignedBusinessEntityId;
        e.ModifiedDate = DateTime.UtcNow;
    }

    public static MaintenanceWorkOrderTaskDto ToDto(this MaintenanceWorkOrderTask e) => new(
        e.Id, e.MaintenanceWorkOrderId, e.SequenceNumber, e.TaskName,
        e.Instructions, e.EstimatedMinutes, e.ActualMinutes,
        e.RequiresSignoff, e.IsComplete, e.CompletedAt,
        e.CompletedByUserId, e.SignoffNotes, e.ModifiedDate);

    public static MaintenanceWorkOrderTask ToEntity(this CreateWorkOrderTaskRequest r) => new()
    {
        MaintenanceWorkOrderId = r.MaintenanceWorkOrderId,
        SequenceNumber = r.SequenceNumber,
        TaskName = (r.TaskName ?? string.Empty).Trim(),
        Instructions = r.Instructions?.Trim(),
        EstimatedMinutes = r.EstimatedMinutes,
        RequiresSignoff = r.RequiresSignoff,
        ModifiedDate = DateTime.UtcNow,
    };
}
