using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AWBlazorApp.Features.Quality.Domain;

/// <summary>
/// Free-form action record appended to a <see cref="NonConformance"/>. Used for the
/// investigation trail — contact attempts, rework notes, dispositions-in-progress, etc.
/// </summary>
[Table("NonConformanceAction", Schema = "qa")]
public class NonConformanceAction
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public int NonConformanceId { get; set; }

    [MaxLength(500)] public string Action { get; set; } = string.Empty;

    [MaxLength(450)] public string? PerformedByUserId { get; set; }
    public DateTime PerformedAt { get; set; }

    [MaxLength(2000)] public string? Notes { get; set; }

    public DateTime ModifiedDate { get; set; }
}
