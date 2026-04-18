using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AWBlazorApp.Features.Performance.Domain;

/// <summary>Per-station daily rollup of throughput, average cycle time, yield.</summary>
[Table("ProductionDailyMetric", Schema = "perf")]
public class ProductionDailyMetric
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    public int StationId { get; set; }

    public DateOnly Date { get; set; }

    [Column(TypeName = "decimal(18,4)")] public decimal UnitsProduced { get; set; }
    [Column(TypeName = "decimal(18,4)")] public decimal UnitsScrapped { get; set; }

    [Column(TypeName = "decimal(10,2)")] public decimal? AverageCycleSeconds { get; set; }

    /// <summary>First-pass yield fraction (0-1). Units produced / (produced + scrapped).</summary>
    [Column(TypeName = "decimal(5,4)")] public decimal? YieldFraction { get; set; }

    public int RunCount { get; set; }

    public DateTime ComputedAt { get; set; }

    public DateTime ModifiedDate { get; set; }
}
