using AWBlazorApp.Features.AdventureWorks.Domain;
using System.ComponentModel.DataAnnotations;

namespace AWBlazorApp.Features.Maintenance.Domain;

public class MaintenanceWorkOrderAuditLog : AdventureWorksAuditLogBase
{
    public int MaintenanceWorkOrderId { get; set; }

    [MaxLength(32)] public string? WorkOrderNumber { get; set; }
    [MaxLength(200)] public string? Title { get; set; }
    [MaxLength(4000)] public string? Description { get; set; }
    public int AssetId { get; set; }
    public WorkOrderType Type { get; set; }
    public WorkOrderStatus Status { get; set; }
    public WorkOrderPriority Priority { get; set; }
    public int? PmScheduleId { get; set; }
    public DateTime? ScheduledFor { get; set; }
    public int? AssignedBusinessEntityId { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? HeldAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    [MaxLength(2000)] public string? CompletionNotes { get; set; }
    [MaxLength(450)] public string? RaisedByUserId { get; set; }
    public DateTime RaisedAt { get; set; }
    public decimal? CompletedMeterValue { get; set; }
    public DateTime SourceModifiedDate { get; set; }
}
