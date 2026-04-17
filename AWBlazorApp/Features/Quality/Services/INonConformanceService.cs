using AWBlazorApp.Features.Quality.Domain;

namespace AWBlazorApp.Features.Quality.Services;

/// <summary>
/// NCR disposition handler. <see cref="DispositionAsync"/> sets the disposition + status →
/// Dispositioned, and for Scrap and Quarantine dispositions posts the corresponding
/// inventory transaction (SCRAP, or paired Available→Quarantine MOVE) through
/// <c>IInventoryService</c>. Other dispositions (Rework, UseAsIs, ReturnToSupplier) record
/// the choice but don't touch inventory — the human follow-up workflow handles those.
/// </summary>
public interface INonConformanceService
{
    Task DispositionAsync(int nonConformanceId, NonConformanceDisposition disposition,
        string? notes, string? userId, CancellationToken cancellationToken);
}
