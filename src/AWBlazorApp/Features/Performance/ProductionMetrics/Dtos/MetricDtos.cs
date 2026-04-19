using AWBlazorApp.Features.Performance.Kpis.Domain; using AWBlazorApp.Features.Performance.MaintenanceMetrics.Domain; using AWBlazorApp.Features.Performance.Oee.Domain; using AWBlazorApp.Features.Performance.ProductionMetrics.Domain; using AWBlazorApp.Features.Performance.Reports.Domain; using AWBlazorApp.Features.Performance.Scorecards.Domain; 

namespace AWBlazorApp.Features.Performance.ProductionMetrics.Dtos;

public sealed record OeeSnapshotDto(
    long Id, int StationId, PerformancePeriodKind PeriodKind,
    DateTime PeriodStart, DateTime PeriodEnd,
    decimal PlannedRuntimeMinutes, decimal ActualRuntimeMinutes, decimal DowntimeMinutes,
    decimal UnitsProduced, decimal UnitsScrapped, decimal IdealCycleSeconds,
    decimal Availability, decimal Performance, decimal Quality, decimal Oee,
    DateTime ComputedAt, DateTime ModifiedDate);

public sealed record ComputeOeeRequest
{
    public int StationId { get; set; }
    public PerformancePeriodKind PeriodKind { get; set; } = PerformancePeriodKind.Day;
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public decimal IdealCycleSeconds { get; set; }
}

public sealed record ProductionDailyMetricDto(
    long Id, int StationId, DateOnly Date,
    decimal UnitsProduced, decimal UnitsScrapped,
    decimal? AverageCycleSeconds, decimal? YieldFraction,
    int RunCount, DateTime ComputedAt, DateTime ModifiedDate);

public sealed record ComputeProductionMetricRequest
{
    public int StationId { get; set; }
    public DateOnly Date { get; set; }
}

public sealed record MaintenanceMonthlyMetricDto(
    long Id, int AssetId, int Year, int Month,
    int WorkOrderCount, int BreakdownCount,
    int PmWorkOrderCount, int PmCompletedCount,
    decimal? MtbfHours, decimal? MttrHours,
    decimal? AvailabilityFraction, decimal? PmComplianceFraction,
    DateTime ComputedAt, DateTime ModifiedDate);

public sealed record ComputeMaintenanceMetricRequest
{
    public int AssetId { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
}

public static class MetricMappings
{
    public static OeeSnapshotDto ToDto(this OeeSnapshot e) => new(
        e.Id, e.StationId, e.PeriodKind,
        e.PeriodStart, e.PeriodEnd,
        e.PlannedRuntimeMinutes, e.ActualRuntimeMinutes, e.DowntimeMinutes,
        e.UnitsProduced, e.UnitsScrapped, e.IdealCycleSeconds,
        e.Availability, e.Performance, e.Quality, e.Oee,
        e.ComputedAt, e.ModifiedDate);

    public static ProductionDailyMetricDto ToDto(this ProductionDailyMetric e) => new(
        e.Id, e.StationId, e.Date,
        e.UnitsProduced, e.UnitsScrapped,
        e.AverageCycleSeconds, e.YieldFraction,
        e.RunCount, e.ComputedAt, e.ModifiedDate);

    public static MaintenanceMonthlyMetricDto ToDto(this MaintenanceMonthlyMetric e) => new(
        e.Id, e.AssetId, e.Year, e.Month,
        e.WorkOrderCount, e.BreakdownCount,
        e.PmWorkOrderCount, e.PmCompletedCount,
        e.MtbfHours, e.MttrHours,
        e.AvailabilityFraction, e.PmComplianceFraction,
        e.ComputedAt, e.ModifiedDate);
}
