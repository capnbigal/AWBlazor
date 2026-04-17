using AWBlazorApp.Features.Quality.Domain;

namespace AWBlazorApp.Features.Quality.Services;

/// <summary>
/// CAPA case lifecycle. Linear stage progression from Open → Investigation → CorrectiveAction
/// → Verification → Closed. Each transition stamps timestamps and lets the caller fill in the
/// stage's notes (RootCause, CorrectiveAction body, etc.).
/// </summary>
public interface ICapaService
{
    Task<int> OpenAsync(string title, int? ownerBusinessEntityId, IEnumerable<int> linkedNcrIds, string? userId, CancellationToken cancellationToken);
    Task LinkNcrAsync(int capaCaseId, int nonConformanceId, CancellationToken cancellationToken);
    Task UnlinkNcrAsync(int capaCaseId, int nonConformanceId, CancellationToken cancellationToken);
    Task TransitionAsync(int capaCaseId, CapaStatus targetStatus, string? userId, CancellationToken cancellationToken);
}
