using AWBlazorApp.Features.Dashboard.Dtos;

namespace AWBlazorApp.Features.Dashboard.Services;

/// <summary>
/// Aggregates a single snapshot for the cross-module plant dashboard. Backed by
/// IMemoryCache (5 min TTL) so a refresh-spam doesn't pound the database.
/// </summary>
public interface IPlantDashboardService
{
    Task<PlantDashboardDto> GetAsync(CancellationToken cancellationToken);

    /// <summary>Force a cache miss on the next call. Useful for ops endpoints.</summary>
    void Invalidate();
}
