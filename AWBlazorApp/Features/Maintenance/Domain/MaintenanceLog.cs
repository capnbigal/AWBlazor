using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AWBlazorApp.Features.Maintenance.Domain;

/// <summary>
/// Free-form maintenance log entry — observations, near-misses, warnings not yet escalated
/// to a work order. Lightweight companion to the formal WO flow.
/// </summary>
[Table("MaintenanceLog", Schema = "maint")]
public class MaintenanceLog
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    public int AssetId { get; set; }

    public MaintenanceLogKind Kind { get; set; } = MaintenanceLogKind.Observation;

    [MaxLength(2000)] public string Note { get; set; } = string.Empty;

    [MaxLength(450)] public string? AuthoredByUserId { get; set; }
    public DateTime AuthoredAt { get; set; }

    /// <summary>Optional link to a work order that this log entry led to.</summary>
    public int? MaintenanceWorkOrderId { get; set; }

    public DateTime ModifiedDate { get; set; }
}

public enum MaintenanceLogKind : byte
{
    Observation = 1,
    Warning = 2,
    Incident = 3,
    NearMiss = 4,
}
