using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AWBlazorApp.Features.Maintenance.PmSchedules.Domain;

/// <summary>
/// Preventive maintenance template. Triggers when the interval has passed since the last
/// completed WO of this schedule. The interval kind controls how "passed" is measured:
/// <see cref="PmIntervalKind.Days"/> is wall-clock; <see cref="PmIntervalKind.RuntimeHours"/>
/// and <see cref="PmIntervalKind.Cycles"/> use <see cref="MeterReading"/> deltas.
/// </summary>
[Table("PmSchedule", Schema = "maint")]
public class PmSchedule
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [MaxLength(32)] public string Code { get; set; } = string.Empty;
    [MaxLength(200)] public string Name { get; set; } = string.Empty;
    [MaxLength(2000)] public string? Description { get; set; }

    public int AssetId { get; set; }

    public PmIntervalKind IntervalKind { get; set; } = PmIntervalKind.Days;

    /// <summary>Interval magnitude. Days when Kind=Days, hours when Kind=RuntimeHours, cycles when Kind=Cycles.</summary>
    public int IntervalValue { get; set; }

    public WorkOrderPriority DefaultPriority { get; set; } = WorkOrderPriority.Medium;

    public int EstimatedMinutes { get; set; }

    public bool IsActive { get; set; } = true;

    /// <summary>Cached timestamp of the most recently Completed work order generated from this schedule. Updated by PmScheduleService.</summary>
    public DateTime? LastCompletedAt { get; set; }

    /// <summary>Cached meter value at the time of the most recently completed work order (for meter-based schedules).</summary>
    public decimal? LastCompletedMeterValue { get; set; }

    public DateTime ModifiedDate { get; set; }
}

public enum PmIntervalKind : byte
{
    Days = 1,
    RuntimeHours = 2,
    Cycles = 3,
}

public enum WorkOrderPriority : byte
{
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4,
}
