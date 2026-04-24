using AWBlazorApp.Infrastructure.Persistence;

namespace AWBlazorApp.Features.Processes.Timelines.Application;

public interface IChainHopQuery
{
    string ParentEntity { get; }
    string ChildEntity { get; }
    string ForeignKey { get; }

    Task<IReadOnlyList<string>> GetChildIdsAsync(
        ApplicationDbContext db, IReadOnlyList<string> parentIds, CancellationToken ct);

    Task<string?> GetParentIdAsync(
        ApplicationDbContext db, string childId, CancellationToken ct);
}
