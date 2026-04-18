using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AWBlazorApp.Features.Maintenance.Domain;

/// <summary>Ordered task in a <see cref="PmSchedule"/> — gets copied onto the generated WO's tasks.</summary>
[Table("PmScheduleTask", Schema = "maint")]
public class PmScheduleTask
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public int PmScheduleId { get; set; }

    public int SequenceNumber { get; set; }

    [MaxLength(200)] public string TaskName { get; set; } = string.Empty;

    [MaxLength(2000)] public string? Instructions { get; set; }

    public int? EstimatedMinutes { get; set; }

    public bool RequiresSignoff { get; set; }

    public DateTime ModifiedDate { get; set; }
}
