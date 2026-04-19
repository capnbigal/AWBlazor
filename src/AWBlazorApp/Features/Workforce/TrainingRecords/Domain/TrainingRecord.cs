using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AWBlazorApp.Features.Workforce.TrainingRecords.Domain;

/// <summary>
/// One completion of a <see cref="TrainingCourse"/> by an employee. <see cref="ExpiresOn"/> is
/// stamped from <c>CompletedAt + Course.RecurrenceMonths</c> when the course is recurrent;
/// stays null otherwise. Append-only — recompletions create a new row.
/// </summary>
[Table("TrainingRecord", Schema = "wf")]
public class TrainingRecord
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public int TrainingCourseId { get; set; }

    /// <summary>FK → <c>HumanResources.Employee.BusinessEntityID</c>.</summary>
    public int BusinessEntityId { get; set; }

    public DateTime CompletedAt { get; set; }
    public DateTime? ExpiresOn { get; set; }

    [MaxLength(200)] public string? Score { get; set; }
    [MaxLength(500)] public string? EvidenceUrl { get; set; }
    [MaxLength(500)] public string? Notes { get; set; }

    [MaxLength(450)] public string? RecordedByUserId { get; set; }

    public DateTime ModifiedDate { get; set; }
}
