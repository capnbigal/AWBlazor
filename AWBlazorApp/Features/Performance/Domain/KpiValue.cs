using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AWBlazorApp.Features.Performance.Domain;

/// <summary>Computed KPI value for a specific period — written by <c>IKpiEvaluationService</c>.</summary>
[Table("KpiValue", Schema = "perf")]
public class KpiValue
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    public int KpiDefinitionId { get; set; }

    public PerformancePeriodKind PeriodKind { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }

    [Column(TypeName = "decimal(18,4)")] public decimal? Value { get; set; }

    [Column(TypeName = "decimal(18,4)")] public decimal? TargetAtPeriod { get; set; }

    public KpiStatus Status { get; set; } = KpiStatus.Unknown;

    public DateTime ComputedAt { get; set; }

    public DateTime ModifiedDate { get; set; }
}

public enum KpiStatus : byte
{
    Unknown = 0,
    OnTarget = 1,
    Warning = 2,
    Critical = 3,
}
