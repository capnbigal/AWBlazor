using AWBlazorApp.Features.AdventureWorks.Domain;
using System.ComponentModel.DataAnnotations;

namespace AWBlazorApp.Features.Quality.Domain;

public class InspectionAuditLog : AdventureWorksAuditLogBase
{
    public int InspectionId { get; set; }

    [MaxLength(32)] public string? InspectionNumber { get; set; }
    public int InspectionPlanId { get; set; }
    public InspectionStatus Status { get; set; }
    public InspectionSourceKind SourceKind { get; set; }
    public int SourceId { get; set; }
    public int? InspectorBusinessEntityId { get; set; }
    public DateTime? InspectedAt { get; set; }
    public int? InventoryItemId { get; set; }
    public int? LotId { get; set; }
    public decimal Quantity { get; set; }
    [MaxLength(3)] public string? UnitMeasureCode { get; set; }
    [MaxLength(500)] public string? Notes { get; set; }
    [MaxLength(450)] public string? PostedByUserId { get; set; }
    public DateTime SourceModifiedDate { get; set; }
}
