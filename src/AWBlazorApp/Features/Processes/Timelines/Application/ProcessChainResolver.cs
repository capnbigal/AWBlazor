using System.Text.Json;
using AWBlazorApp.Features.Processes.Timelines.Domain;
using AWBlazorApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.Processes.Timelines.Application;

public class ProcessChainResolver : IProcessChainResolver
{
    private readonly IDbContextFactory<ApplicationDbContext> _factory;
    private readonly IEnumerable<IChainHopQuery> _hops;

    public ProcessChainResolver(
        IDbContextFactory<ApplicationDbContext> factory,
        IEnumerable<IChainHopQuery> hops)
        => (_factory, _hops) = (factory, hops);

    public async Task<ChainInstance> ResolveAsync(string chainCode, string rootEntityId, CancellationToken ct)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);
        var def = await db.ProcessChainDefinitions.AsNoTracking()
            .SingleOrDefaultAsync(c => c.Code == chainCode && c.IsActive, ct)
            ?? throw new ChainDefinitionNotFoundException(chainCode);

        var steps = JsonSerializer.Deserialize<ChainStep[]>(def.StepsJson,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })
            ?? Array.Empty<ChainStep>();

        var collected = new Dictionary<string, IReadOnlyList<string>>(StringComparer.Ordinal);
        foreach (var step in steps)
        {
            if (step.Role == ChainStep.RoleRoot)
            {
                collected[step.Entity] = new[] { rootEntityId };
                continue;
            }
            if (step.ParentEntity is null || step.ForeignKey is null)
                throw new InvalidOperationException(
                    $"Step for {step.Entity} has Role=Child but missing ParentEntity or ForeignKey.");

            var parentIds = collected.TryGetValue(step.ParentEntity, out var p)
                ? p : Array.Empty<string>();
            if (parentIds.Count == 0)
            {
                collected[step.Entity] = Array.Empty<string>();
                continue;
            }

            var hop = _hops.FirstOrDefault(h =>
                h.ParentEntity == step.ParentEntity &&
                h.ChildEntity  == step.Entity &&
                h.ForeignKey   == step.ForeignKey)
                ?? throw new ChainStepNotSupportedException(step.ParentEntity, step.Entity, step.ForeignKey);

            collected[step.Entity] = await hop.GetChildIdsAsync(db, parentIds, ct);
        }

        return new ChainInstance(def, rootEntityId, collected);
    }

    public Task<IReadOnlyList<ChainInstanceSummary>> RecentAsync(ChainQuery query, CancellationToken ct)
        => throw new NotImplementedException("Implemented in Task 9.");
}
