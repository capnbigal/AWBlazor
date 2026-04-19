using AWBlazorApp.Shared.Domain;
using System.ComponentModel.DataAnnotations;

namespace AWBlazorApp.Features.Performance.Domain;

public class KpiDefinitionAuditLog : AdventureWorksAuditLogBase
{
    public int KpiDefinitionId { get; set; }

    [MaxLength(32)] public string? Code { get; set; }
    [MaxLength(200)] public string? Name { get; set; }
    [MaxLength(2000)] public string? Description { get; set; }
    [MaxLength(32)] public string? Unit { get; set; }
    public KpiSource Source { get; set; }
    public KpiAggregation Aggregation { get; set; }
    public decimal? TargetValue { get; set; }
    public decimal? WarningThreshold { get; set; }
    public decimal? CriticalThreshold { get; set; }
    public KpiDirection Direction { get; set; }
    public bool IsActive { get; set; }
    public DateTime SourceModifiedDate { get; set; }
}
