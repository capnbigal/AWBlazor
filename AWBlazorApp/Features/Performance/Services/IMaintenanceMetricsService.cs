using AWBlazorApp.Features.Performance.Domain;

namespace AWBlazorApp.Features.Performance.Services;

/// <summary>
/// Computes per-asset monthly maintenance metrics (MTBF, MTTR, availability, PM compliance)
/// from <c>maint.MaintenanceWorkOrder</c>.
/// </summary>
public interface IMaintenanceMetricsService
{
    Task<MaintenanceMonthlyMetric> ComputeMonthlyAsync(int assetId, int year, int month, CancellationToken cancellationToken);
}
