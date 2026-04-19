using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AWBlazorApp.Features.Workforce.Attendance.Domain;

/// <summary>
/// Per-shift attendance. Distinct from <c>mes.OperatorClockEvent</c> (per-station, per-run,
/// drives OEE Performance) — this drives payroll / HR reporting. <see cref="ShiftId"/>
/// references AdventureWorks' <c>HumanResources.Shift</c>.
/// </summary>
[Table("AttendanceEvent", Schema = "wf")]
public class AttendanceEvent
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    /// <summary>FK → <c>HumanResources.Employee.BusinessEntityID</c>.</summary>
    public int BusinessEntityId { get; set; }

    /// <summary>FK → <c>HumanResources.Shift.ShiftID</c> (byte PK, model as int for convenience).</summary>
    public int? ShiftId { get; set; }

    public DateOnly ShiftDate { get; set; }

    public DateTime? ClockInAt { get; set; }
    public DateTime? ClockOutAt { get; set; }

    public AttendanceStatus Status { get; set; } = AttendanceStatus.Present;

    [MaxLength(500)] public string? Notes { get; set; }

    public DateTime ModifiedDate { get; set; }
}

public enum AttendanceStatus : byte
{
    Present = 1,
    Late = 2,
    Absent = 3,
    Excused = 4,
}
