using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AWBlazorApp.Features.Mes.Runs.Domain;

/// <summary>
/// One execution of a work order (or an unplanned ad-hoc run) at a station. Completing a run
/// posts WIP_ISSUE (materials consumed) and WIP_RECEIPT (finished goods) inventory transactions
/// via <c>IInventoryService</c>. <see cref="Kind"/> distinguishes a proper planned production
/// run from engineering builds, service rework, replacement runs, and other ad-hoc types —
/// the WorkOrder FK is required when Kind is Production and optional otherwise.
/// </summary>
[Table("ProductionRun", Schema = "mes")]
public class ProductionRun
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [MaxLength(32)]
    public string RunNumber { get; set; } = string.Empty;

    public ProductionRunKind Kind { get; set; } = ProductionRunKind.Production;

    /// <summary>FK → <c>Production.WorkOrder.WorkOrderID</c>. Required when Kind == Production.</summary>
    public int? WorkOrderId { get; set; }

    public int? StationId { get; set; }
    public int? AssetId { get; set; }

    public ProductionRunStatus Status { get; set; } = ProductionRunStatus.Draft;

    public DateTime? PlannedStartAt { get; set; }
    public DateTime? ActualStartAt { get; set; }
    public DateTime? ActualEndAt { get; set; }

    [Column(TypeName = "decimal(18,4)")] public decimal QuantityPlanned { get; set; }
    [Column(TypeName = "decimal(18,4)")] public decimal QuantityProduced { get; set; }
    [Column(TypeName = "decimal(18,4)")] public decimal QuantityScrapped { get; set; }

    [MaxLength(500)] public string? Notes { get; set; }

    [MaxLength(450)] public string? PostedByUserId { get; set; }

    public DateTime ModifiedDate { get; set; }
}

public enum ProductionRunKind : byte
{
    Production = 1,
    Engineering = 2,
    Replacement = 3,
    Service = 4,
    Other = 5,
}

public enum ProductionRunStatus : byte
{
    Draft = 1,
    InProgress = 2,
    Paused = 3,
    Completed = 4,
    Cancelled = 5,
}
