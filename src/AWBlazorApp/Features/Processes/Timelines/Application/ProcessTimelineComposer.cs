using AWBlazorApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.Processes.Timelines.Application;

public class ProcessTimelineComposer : IProcessTimelineComposer
{
    private const int MaxEvents = 500;
    private readonly IDbContextFactory<ApplicationDbContext> _factory;

    public ProcessTimelineComposer(IDbContextFactory<ApplicationDbContext> factory)
        => _factory = factory;

    public async Task<ProcessTimeline> ComposeAsync(ChainInstance instance, CancellationToken ct)
    {
        var totalIds = instance.CollectedIds.Sum(kvp => kvp.Value.Count);
        if (totalIds == 0)
            return new ProcessTimeline(instance, Array.Empty<TimelineEvent>(), Truncated: false);

        await using var db = await _factory.CreateDbContextAsync(ct);

        // OR-of-ANDs: one query per entity type (union client-side). For slice-1 volumes
        // (few hundred IDs per chain) this is fast and avoids UNION-ALL SQL translation quirks.
        var events = new List<TimelineEvent>(MaxEvents + 1);
        foreach (var kvp in instance.CollectedIds)
        {
            if (kvp.Value.Count == 0) continue;
            var entityType = kvp.Key;
            var ids = kvp.Value.ToArray();
            var batch = await db.AuditLogs.AsNoTracking()
                .Where(a => a.EntityType == entityType && ids.Contains(a.EntityId))
                .Select(a => new TimelineEvent(
                    a.Id, a.EntityType, a.EntityId, a.Action, a.ChangedDate,
                    a.ChangedBy, a.Summary, a.ChangesJson))
                .ToListAsync(ct);
            events.AddRange(batch);
        }

        var ordered = events.OrderBy(e => e.At).ToList();
        var truncated = ordered.Count > MaxEvents;
        if (truncated) ordered = ordered.Take(MaxEvents).ToList();
        return new ProcessTimeline(instance, ordered, truncated);
    }
}
