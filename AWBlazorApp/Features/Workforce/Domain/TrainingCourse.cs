using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AWBlazorApp.Features.Workforce.Domain;

/// <summary>
/// A training course employees can complete. <see cref="RecurrenceMonths"/> when set drives
/// the expiry computation on <see cref="TrainingRecord"/>: a record's <c>ExpiresOn</c> is
/// stamped at <c>CompletedAt + RecurrenceMonths</c>. Null = one-time training that never
/// expires.
/// </summary>
[Table("TrainingCourse", Schema = "wf")]
public class TrainingCourse
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [MaxLength(32)] public string Code { get; set; } = string.Empty;
    [MaxLength(200)] public string Name { get; set; } = string.Empty;
    [MaxLength(2000)] public string? Description { get; set; }

    public int? DurationMinutes { get; set; }

    /// <summary>If set, training records expire this many months after completion. Null = no expiry.</summary>
    public int? RecurrenceMonths { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime ModifiedDate { get; set; }
}
