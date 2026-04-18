using AWBlazorApp.Features.Performance.Domain;

namespace AWBlazorApp.Features.Performance.Services;

/// <summary>Computes per-station daily production metrics from <c>mes.ProductionRun</c>.</summary>
public interface IProductionMetricsService
{
    Task<ProductionDailyMetric> ComputeDailyAsync(int stationId, DateOnly date, CancellationToken cancellationToken);
}
