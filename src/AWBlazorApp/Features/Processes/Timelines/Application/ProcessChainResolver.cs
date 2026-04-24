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

    public async Task<IReadOnlyList<ChainInstanceSummary>> RecentAsync(ChainQuery query, CancellationToken ct)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);

        // Load active chain definitions, optionally filtered by ChainCode
        var chainsQuery = db.ProcessChainDefinitions.AsNoTracking().Where(c => c.IsActive);
        if (!string.IsNullOrWhiteSpace(query.ChainCode))
            chainsQuery = chainsQuery.Where(c => c.Code == query.ChainCode);
        var chains = await chainsQuery.ToListAsync(ct);
        if (chains.Count == 0) return Array.Empty<ChainInstanceSummary>();

        // Build entity-type → (chainCode, rootEntity, steps) lookup
        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var entityToChain = new Dictionary<string, (string ChainCode, string RootEntity, ChainStep[] Steps)>(StringComparer.Ordinal);
        foreach (var def in chains)
        {
            var steps = JsonSerializer.Deserialize<ChainStep[]>(def.StepsJson, options) ?? Array.Empty<ChainStep>();
            var root = steps.FirstOrDefault(s => s.Role == ChainStep.RoleRoot);
            if (root is null) continue;
            foreach (var step in steps)
                entityToChain[step.Entity] = (def.Code, root.Entity, steps);
        }
        if (entityToChain.Count == 0) return Array.Empty<ChainInstanceSummary>();

        // Phase 1: query AuditLog in date range for chain-relevant entity types
        var since = query.Since ?? DateTime.UtcNow.AddDays(-30);
        var until = query.Until ?? DateTime.UtcNow;
        var entityTypes = entityToChain.Keys.ToList();

        var events = await db.AuditLogs.AsNoTracking()
            .Where(a => a.ChangedDate >= since && a.ChangedDate <= until
                     && entityTypes.Contains(a.EntityType))
            .Select(a => new { a.EntityType, a.EntityId, a.ChangedBy, a.ChangedDate })
            .ToListAsync(ct);
        if (events.Count == 0) return Array.Empty<ChainInstanceSummary>();

        // Phase 2: reverse-walk each event up to its root
        var rootCache = new Dictionary<(string, string), string?>();
        async Task<(string ChainCode, string RootEntity, string? RootId)?> ResolveToRoot(string entity, string id)
        {
            if (!entityToChain.TryGetValue(entity, out var meta)) return null;
            var current = (entity: entity, id: id);
            while (current.entity != meta.RootEntity)
            {
                if (rootCache.TryGetValue(current, out var cached))
                {
                    if (cached is null) return null;
                    return (meta.ChainCode, meta.RootEntity, cached);
                }
                var step = meta.Steps.FirstOrDefault(s => s.Entity == current.entity && s.Role == ChainStep.RoleChild);
                if (step?.ParentEntity is null || step.ForeignKey is null) return null;
                var hop = _hops.FirstOrDefault(h =>
                    h.ParentEntity == step.ParentEntity &&
                    h.ChildEntity  == current.entity &&
                    h.ForeignKey   == step.ForeignKey);
                if (hop is null) return null;
                var parentId = await hop.GetParentIdAsync(db, current.id, ct);
                rootCache[current] = parentId;
                if (parentId is null) return null;
                current = (step.ParentEntity, parentId);
            }
            return (meta.ChainCode, meta.RootEntity, current.id);
        }

        // Group events by resolved (chainCode, rootId)
        var buckets = new Dictionary<(string ChainCode, string RootId), List<(DateTime At, string? Who)>>();
        foreach (var ev in events)
        {
            var resolved = await ResolveToRoot(ev.EntityType, ev.EntityId);
            if (resolved?.RootId is null) continue;
            var key = (resolved.Value.ChainCode, resolved.Value.RootId);
            if (!buckets.TryGetValue(key, out var list))
                buckets[key] = list = new List<(DateTime, string?)>();
            list.Add((ev.ChangedDate, ev.ChangedBy));
        }

        // Materialize, filter, cap, order
        var limit = Math.Clamp(query.Limit, 1, 500);
        var summaries = buckets
            .Select(kvp => new ChainInstanceSummary(
                ChainCode: kvp.Key.ChainCode,
                RootEntityId: kvp.Key.RootId,
                RootLabel: null,
                FirstEventAt: kvp.Value.Min(x => x.At),
                LastEventAt:  kvp.Value.Max(x => x.At),
                EventCount:   kvp.Value.Count,
                ContributorUsers: kvp.Value.Select(x => x.Who).Where(x => x != null).Distinct().Select(x => x!).ToArray()))
            .Where(s => string.IsNullOrWhiteSpace(query.Owner) || s.ContributorUsers.Contains(query.Owner!))
            .OrderByDescending(s => s.LastEventAt)
            .Take(limit)
            .ToList();

        return summaries;
    }
}
