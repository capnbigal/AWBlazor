using AWBlazorApp.Infrastructure.Persistence;

namespace AWBlazorApp.Features.Processes.Timelines.Application;

public interface IRootEntityLabeler
{
    string EntityType { get; }
    Task<string?> GetLabelAsync(ApplicationDbContext db, string rootEntityId, CancellationToken ct);
}
