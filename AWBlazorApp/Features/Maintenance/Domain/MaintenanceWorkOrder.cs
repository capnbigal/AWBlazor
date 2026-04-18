using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AWBlazorApp.Features.Maintenance.Domain;

/// <summary>
/// Maintenance work order. State machine:
///   Draft → Scheduled → InProgress → Completed
/// with OnHold (from InProgress) and Cancelled (from Draft / Scheduled / InProgress) side paths.
/// <see cref="PmScheduleId"/> is set when the WO was generated from a PM schedule.
/// </summary>
[Table("MaintenanceWorkOrder", Schema = "maint")]
public class MaintenanceWorkOrder
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [MaxLength(32)] public string WorkOrderNumber { get; set; } = string.Empty;

    [MaxLength(200)] public string Title { get; set; } = string.Empty;

    [MaxLength(4000)] public string? Description { get; set; }

    public int AssetId { get; set; }

    public WorkOrderType Type { get; set; } = WorkOrderType.Corrective;
    public WorkOrderStatus Status { get; set; } = WorkOrderStatus.Draft;
    public WorkOrderPriority Priority { get; set; } = WorkOrderPriority.Medium;

    /// <summary>Set when generated from a PM schedule — links back for metrics and next-due updates.</summary>
    public int? PmScheduleId { get; set; }

    public DateTime? ScheduledFor { get; set; }

    /// <summary>FK → <c>HumanResources.Employee.BusinessEntityID</c>. Assigned technician.</summary>
    public int? AssignedBusinessEntityId { get; set; }

    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? HeldAt { get; set; }
    public DateTime? CancelledAt { get; set; }

    [MaxLength(2000)] public string? CompletionNotes { get; set; }

    [MaxLength(450)] public string? RaisedByUserId { get; set; }
    public DateTime RaisedAt { get; set; }

    /// <summary>Cached meter reading at completion, for schedules that trigger on runtime hours / cycles.</summary>
    public decimal? CompletedMeterValue { get; set; }

    public DateTime ModifiedDate { get; set; }
}

public enum WorkOrderType : byte
{
    Preventive = 1,
    Corrective = 2,
    Breakdown = 3,
    Inspection = 4,
    Safety = 5,
}

public enum WorkOrderStatus : byte
{
    Draft = 1,
    Scheduled = 2,
    InProgress = 3,
    OnHold = 4,
    Completed = 5,
    Cancelled = 6,
}
