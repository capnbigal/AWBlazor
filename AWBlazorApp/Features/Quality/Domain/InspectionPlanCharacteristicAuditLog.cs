using AWBlazorApp.Features.AdventureWorks.Domain;
using System.ComponentModel.DataAnnotations;

namespace AWBlazorApp.Features.Quality.Domain;

public class InspectionPlanCharacteristicAuditLog : AdventureWorksAuditLogBase
{
    public int InspectionPlanCharacteristicId { get; set; }

    public int InspectionPlanId { get; set; }
    public int SequenceNumber { get; set; }
    [MaxLength(200)] public string? Name { get; set; }
    public CharacteristicKind Kind { get; set; }
    public decimal? MinValue { get; set; }
    public decimal? MaxValue { get; set; }
    public decimal? TargetValue { get; set; }
    [MaxLength(3)] public string? UnitMeasureCode { get; set; }
    [MaxLength(100)] public string? ExpectedValue { get; set; }
    public bool IsCritical { get; set; }
    public DateTime SourceModifiedDate { get; set; }
}
