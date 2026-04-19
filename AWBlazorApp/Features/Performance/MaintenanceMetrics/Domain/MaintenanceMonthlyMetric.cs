using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AWBlazorApp.Features.Performance.MaintenanceMetrics.Domain;

/// <summary>Per-asset monthly rollup: MTBF, MTTR, availability, PM compliance.</summary>
[Table("MaintenanceMonthlyMetric", Schema = "perf")]
public class MaintenanceMonthlyMetric
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    public int AssetId { get; set; }

    public int Year { get; set; }
    public int Month { get; set; }

    public int WorkOrderCount { get; set; }
    public int BreakdownCount { get; set; }
    public int PmWorkOrderCount { get; set; }
    public int PmCompletedCount { get; set; }

    /// <summary>Mean time between failures, hours. Null when fewer than 2 failures recorded.</summary>
    [Column(TypeName = "decimal(10,2)")] public decimal? MtbfHours { get; set; }

    /// <summary>Mean time to repair, hours. Null when no completed repair WOs in the period.</summary>
    [Column(TypeName = "decimal(10,2)")] public decimal? MttrHours { get; set; }

    /// <summary>Fraction 0-1. Non-breakdown time / total time in the month.</summary>
    [Column(TypeName = "decimal(5,4)")] public decimal? AvailabilityFraction { get; set; }

    /// <summary>Fraction 0-1. Completed PM WOs / total PM WOs scheduled in the month.</summary>
    [Column(TypeName = "decimal(5,4)")] public decimal? PmComplianceFraction { get; set; }

    public DateTime ComputedAt { get; set; }

    public DateTime ModifiedDate { get; set; }
}
