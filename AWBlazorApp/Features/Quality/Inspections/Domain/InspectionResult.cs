using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AWBlazorApp.Features.Quality.Inspections.Domain;

/// <summary>
/// One recorded measurement for one characteristic on an inspection. Append-only — corrections
/// post a new result row with a note rather than updating the old one. <see cref="NumericResult"/>
/// is populated for numeric characteristics; <see cref="AttributeResult"/> for attribute-kind.
/// <see cref="Passed"/> is computed at record time by the service layer against the plan's
/// tolerance fields and snapshotted here so the complete-inspection rollup is cheap.
/// </summary>
[Table("InspectionResult", Schema = "qa")]
public class InspectionResult
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    public int InspectionId { get; set; }

    public int InspectionPlanCharacteristicId { get; set; }

    [Column(TypeName = "decimal(18,4)")]
    public decimal? NumericResult { get; set; }

    [MaxLength(100)] public string? AttributeResult { get; set; }

    public bool Passed { get; set; }

    [MaxLength(500)] public string? Notes { get; set; }

    public DateTime RecordedAt { get; set; }

    public int? RecordedByBusinessEntityId { get; set; }
}
