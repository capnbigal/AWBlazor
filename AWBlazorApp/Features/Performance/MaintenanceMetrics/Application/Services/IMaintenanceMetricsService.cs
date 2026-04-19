using AWBlazorApp.Features.Performance.Kpis.Domain; using AWBlazorApp.Features.Performance.MaintenanceMetrics.Domain; using AWBlazorApp.Features.Performance.Oee.Domain; using AWBlazorApp.Features.Performance.ProductionMetrics.Domain; using AWBlazorApp.Features.Performance.Reports.Domain; using AWBlazorApp.Features.Performance.Scorecards.Domain; 

namespace AWBlazorApp.Features.Performance.MaintenanceMetrics.Application.Services;

/// <summary>
/// Computes per-asset monthly maintenance metrics (MTBF, MTTR, availability, PM compliance)
/// from <c>maint.MaintenanceWorkOrder</c>.
/// </summary>
public interface IMaintenanceMetricsService
{
    Task<MaintenanceMonthlyMetric> ComputeMonthlyAsync(int assetId, int year, int month, CancellationToken cancellationToken);
}
