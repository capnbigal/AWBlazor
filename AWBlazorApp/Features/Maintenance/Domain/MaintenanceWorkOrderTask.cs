using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AWBlazorApp.Features.Maintenance.Domain;

/// <summary>Task within a <see cref="MaintenanceWorkOrder"/>. Populated from the PM schedule's tasks or added ad-hoc for corrective work.</summary>
[Table("MaintenanceWorkOrderTask", Schema = "maint")]
public class MaintenanceWorkOrderTask
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public int MaintenanceWorkOrderId { get; set; }

    public int SequenceNumber { get; set; }

    [MaxLength(200)] public string TaskName { get; set; } = string.Empty;

    [MaxLength(2000)] public string? Instructions { get; set; }

    public int? EstimatedMinutes { get; set; }
    public int? ActualMinutes { get; set; }

    public bool RequiresSignoff { get; set; }

    public bool IsComplete { get; set; }
    public DateTime? CompletedAt { get; set; }
    [MaxLength(450)] public string? CompletedByUserId { get; set; }
    [MaxLength(500)] public string? SignoffNotes { get; set; }

    public DateTime ModifiedDate { get; set; }
}
