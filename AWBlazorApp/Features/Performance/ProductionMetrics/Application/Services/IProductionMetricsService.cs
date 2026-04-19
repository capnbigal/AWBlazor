using AWBlazorApp.Features.Performance.Kpis.Domain; using AWBlazorApp.Features.Performance.MaintenanceMetrics.Domain; using AWBlazorApp.Features.Performance.Oee.Domain; using AWBlazorApp.Features.Performance.ProductionMetrics.Domain; using AWBlazorApp.Features.Performance.Reports.Domain; using AWBlazorApp.Features.Performance.Scorecards.Domain; 

namespace AWBlazorApp.Features.Performance.ProductionMetrics.Application.Services;

/// <summary>Computes per-station daily production metrics from <c>mes.ProductionRun</c>.</summary>
public interface IProductionMetricsService
{
    Task<ProductionDailyMetric> ComputeDailyAsync(int stationId, DateOnly date, CancellationToken cancellationToken);
}
