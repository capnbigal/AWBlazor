using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AWBlazorApp.Features.Performance.Oee.Domain;

/// <summary>
/// Persisted OEE rollup per station per period. Computed on demand by
/// <see cref="Services.IOeeService"/> from <c>mes.ProductionRun</c> +
/// <c>mes.DowntimeEvent</c> + <c>qa.Inspection</c>, then cached here so the scorecard /
/// dashboard doesn't re-aggregate on every render. OEE = Availability × Performance × Quality
/// (all stored as fractions 0-1).
/// </summary>
[Table("OeeSnapshot", Schema = "perf")]
public class OeeSnapshot
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    public int StationId { get; set; }

    public PerformancePeriodKind PeriodKind { get; set; }

    /// <summary>First moment of the period (inclusive), in UTC.</summary>
    public DateTime PeriodStart { get; set; }

    /// <summary>First moment of the next period (exclusive), in UTC.</summary>
    public DateTime PeriodEnd { get; set; }

    [Column(TypeName = "decimal(18,4)")] public decimal PlannedRuntimeMinutes { get; set; }
    [Column(TypeName = "decimal(18,4)")] public decimal ActualRuntimeMinutes { get; set; }
    [Column(TypeName = "decimal(18,4)")] public decimal DowntimeMinutes { get; set; }

    [Column(TypeName = "decimal(18,4)")] public decimal UnitsProduced { get; set; }
    [Column(TypeName = "decimal(18,4)")] public decimal UnitsScrapped { get; set; }
    [Column(TypeName = "decimal(18,4)")] public decimal IdealCycleSeconds { get; set; }

    [Column(TypeName = "decimal(5,4)")] public decimal Availability { get; set; }
    [Column(TypeName = "decimal(5,4)")] public decimal Performance { get; set; }
    [Column(TypeName = "decimal(5,4)")] public decimal Quality { get; set; }
    [Column(TypeName = "decimal(5,4)")] public decimal Oee { get; set; }

    public DateTime ComputedAt { get; set; }

    public DateTime ModifiedDate { get; set; }
}

public enum PerformancePeriodKind : byte
{
    Hour = 1,
    Day = 2,
    Week = 3,
    Month = 4,
    Quarter = 5,
    Year = 6,
}
