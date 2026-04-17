using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AWBlazorApp.Features.Mes.Domain;

/// <summary>
/// Reason code for downtime events. Seeded with 15 canonical codes on first boot (Setup,
/// Material shortage, Machine fault, etc.), fully user-manageable afterward.
/// </summary>
[Table("DowntimeReason", Schema = "mes")]
public class DowntimeReason
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [MaxLength(32)] public string Code { get; set; } = string.Empty;
    [MaxLength(100)] public string Name { get; set; } = string.Empty;
    [MaxLength(500)] public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime ModifiedDate { get; set; }
}

public static class DowntimeReasonCodes
{
    public const string Setup = "SETUP";
    public const string MaterialShortage = "MATERIAL";
    public const string MachineFault = "MACHINE_FAULT";
    public const string OperatorBreak = "OPERATOR_BREAK";
    public const string QualityHold = "QUALITY_HOLD";
    public const string Changeover = "CHANGEOVER";
    public const string Cleaning = "CLEANING";
    public const string MaintenanceScheduled = "MAINT_SCHEDULED";
    public const string MaintenanceUnscheduled = "MAINT_UNSCHEDULED";
    public const string Power = "POWER";
    public const string Tooling = "TOOLING";
    public const string WaitForQc = "WAIT_QC";
    public const string WaitForMaterial = "WAIT_MATERIAL";
    public const string Meeting = "MEETING";
    public const string Other = "OTHER";
}
