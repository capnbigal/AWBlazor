using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AWBlazorApp.Features.Performance.Domain;

/// <summary>
/// A named, re-usable KPI. Evaluates to a single numeric value per period. The
/// combination of <see cref="Source"/> + <see cref="Aggregation"/> controls what gets
/// aggregated and how. Status at a given value is derived from <see cref="TargetValue"/>,
/// <see cref="WarningThreshold"/>, and <see cref="CriticalThreshold"/>.
/// </summary>
[Table("KpiDefinition", Schema = "perf")]
public class KpiDefinition
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [MaxLength(32)] public string Code { get; set; } = string.Empty;
    [MaxLength(200)] public string Name { get; set; } = string.Empty;
    [MaxLength(2000)] public string? Description { get; set; }

    [MaxLength(32)] public string? Unit { get; set; }

    public KpiSource Source { get; set; }
    public KpiAggregation Aggregation { get; set; }

    [Column(TypeName = "decimal(18,4)")] public decimal? TargetValue { get; set; }

    /// <summary>Warning threshold — values below this (for "higher is better" KPIs) or above (for "lower is better") flag Warning.</summary>
    [Column(TypeName = "decimal(18,4)")] public decimal? WarningThreshold { get; set; }

    [Column(TypeName = "decimal(18,4)")] public decimal? CriticalThreshold { get; set; }

    public KpiDirection Direction { get; set; } = KpiDirection.HigherIsBetter;

    public bool IsActive { get; set; } = true;

    public DateTime ModifiedDate { get; set; }
}

/// <summary>
/// Which computed metric stream a KPI reads from. Each maps to one column on one table;
/// the <see cref="KpiAggregation"/> controls how the rows in that column are summarised
/// across the evaluation period.
/// </summary>
public enum KpiSource : byte
{
    OeeOverall = 1,
    OeeAvailability = 2,
    OeePerformance = 3,
    OeeQuality = 4,
    ProductionUnits = 5,
    ProductionYield = 6,
    ProductionCycleSeconds = 7,
    MaintenanceMtbf = 8,
    MaintenanceMttr = 9,
    MaintenanceAvailability = 10,
    MaintenancePmCompliance = 11,
}

public enum KpiAggregation : byte
{
    Sum = 1,
    Average = 2,
    Minimum = 3,
    Maximum = 4,
    Latest = 5,
}

public enum KpiDirection : byte
{
    HigherIsBetter = 1,
    LowerIsBetter = 2,
}
