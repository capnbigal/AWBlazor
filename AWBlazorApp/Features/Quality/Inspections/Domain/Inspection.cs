using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AWBlazorApp.Features.Quality.Inspections.Domain;

/// <summary>
/// An actual inspection event. References a reusable plan plus the source that triggered it
/// (a receipt line, a production run, a shipment line, or ad-hoc). <see cref="SourceKind"/> +
/// <see cref="SourceId"/> is a polymorphic soft link — no hard FK because the source can be
/// in any of three different feature modules. Completing the inspection rolls up results
/// into a Pass/Fail, and Fail auto-opens a <see cref="NonConformance"/>.
/// </summary>
[Table("Inspection", Schema = "qa")]
public class Inspection
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [MaxLength(32)] public string InspectionNumber { get; set; } = string.Empty;

    public int InspectionPlanId { get; set; }

    public InspectionStatus Status { get; set; } = InspectionStatus.Pending;

    public InspectionSourceKind SourceKind { get; set; }
    public int SourceId { get; set; }

    public int? InspectorBusinessEntityId { get; set; }

    public DateTime? InspectedAt { get; set; }

    public int? InventoryItemId { get; set; }
    public int? LotId { get; set; }

    [Column(TypeName = "decimal(18,4)")]
    public decimal Quantity { get; set; }

    [MaxLength(3)]
    public string UnitMeasureCode { get; set; } = "EA";

    [MaxLength(500)] public string? Notes { get; set; }

    [MaxLength(450)] public string? PostedByUserId { get; set; }

    public DateTime ModifiedDate { get; set; }
}

public enum InspectionStatus : byte
{
    Pending = 1,
    InProgress = 2,
    Pass = 3,
    Fail = 4,
    Hold = 5,
}

public enum InspectionSourceKind : byte
{
    Manual = 1,
    GoodsReceiptLine = 2,
    ProductionRun = 3,
    ShipmentLine = 4,
}
