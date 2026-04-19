using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AWBlazorApp.Features.Quality.Plans.Domain;

/// <summary>
/// One line item on an <see cref="InspectionPlan"/>. <see cref="Kind"/> picks numeric (with
/// Min/Max/Target/UoM) or attribute (a textual expected value such as "Pass" or "No defects").
/// Critical characteristics are flagged separately so the inspection complete logic can treat
/// a single critical failure as an overall Fail even if every other characteristic passed.
/// </summary>
[Table("InspectionPlanCharacteristic", Schema = "qa")]
public class InspectionPlanCharacteristic
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public int InspectionPlanId { get; set; }

    public int SequenceNumber { get; set; }

    [MaxLength(200)] public string Name { get; set; } = string.Empty;

    public CharacteristicKind Kind { get; set; } = CharacteristicKind.Numeric;

    // Numeric fields (nullable when Kind == Attribute).
    [Column(TypeName = "decimal(18,4)")] public decimal? MinValue { get; set; }
    [Column(TypeName = "decimal(18,4)")] public decimal? MaxValue { get; set; }
    [Column(TypeName = "decimal(18,4)")] public decimal? TargetValue { get; set; }
    [MaxLength(3)] public string? UnitMeasureCode { get; set; }

    // Attribute fields (nullable when Kind == Numeric).
    [MaxLength(100)] public string? ExpectedValue { get; set; }

    public bool IsCritical { get; set; }

    public DateTime ModifiedDate { get; set; }
}

public enum CharacteristicKind : byte
{
    Numeric = 1,
    Attribute = 2,
}
