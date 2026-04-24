namespace AWBlazorApp.Features.Processes.Timelines.Application;

public interface IProcessChainResolver
{
    Task<ChainInstance> ResolveAsync(string chainCode, string rootEntityId, CancellationToken ct);
    Task<IReadOnlyList<ChainInstanceSummary>> RecentAsync(ChainQuery query, CancellationToken ct);
}
