using AWBlazorApp.Shared.Domain;
using System.ComponentModel.DataAnnotations;

namespace AWBlazorApp.Features.Mes.Runs.Domain;

public class ProductionRunAuditLog : AdventureWorksAuditLogBase
{
    public int ProductionRunId { get; set; }

    [MaxLength(32)] public string? RunNumber { get; set; }
    public ProductionRunKind Kind { get; set; }
    public int? WorkOrderId { get; set; }
    public int? StationId { get; set; }
    public int? AssetId { get; set; }
    public ProductionRunStatus Status { get; set; }
    public DateTime? PlannedStartAt { get; set; }
    public DateTime? ActualStartAt { get; set; }
    public DateTime? ActualEndAt { get; set; }
    public decimal QuantityPlanned { get; set; }
    public decimal QuantityProduced { get; set; }
    public decimal QuantityScrapped { get; set; }
    [MaxLength(500)] public string? Notes { get; set; }
    [MaxLength(450)] public string? PostedByUserId { get; set; }
    public DateTime SourceModifiedDate { get; set; }
}
