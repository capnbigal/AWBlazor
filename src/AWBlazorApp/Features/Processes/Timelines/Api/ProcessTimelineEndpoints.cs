using AWBlazorApp.Features.Processes.Timelines.Application;
using AWBlazorApp.Features.Processes.Timelines.Domain;
using AWBlazorApp.Features.Processes.Timelines.Dtos;
using AWBlazorApp.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace AWBlazorApp.Features.Processes.Timelines.Api;

public static class ProcessTimelineEndpoints
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static IEndpointRouteBuilder MapProcessTimelineEndpoints(this IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/api/processes")
            .WithTags("Processes.Timelines")
            .RequireAuthorization("ApiOrCookie");

        // GET /api/processes/chains
        g.MapGet("/chains", async (ApplicationDbContext db, CancellationToken ct) =>
        {
            var chains = await db.ProcessChainDefinitions.AsNoTracking()
                .Where(c => c.IsActive)
                .OrderBy(c => c.SortOrder).ThenBy(c => c.Name)
                .Select(c => new ChainDescriptorDto(c.Code, c.Name, c.Description))
                .ToListAsync(ct);
            return TypedResults.Ok(chains);
        }).WithName("ListProcessChains");

        // GET /api/processes/chains/{chainCode}/timeline?rootEntityId=...
        g.MapGet("/chains/{chainCode}/timeline", async (
            string chainCode,
            [FromQuery] string? rootEntityId,
            IProcessChainResolver resolver,
            IProcessTimelineComposer composer,
            IEnumerable<IRootEntityLabeler> labelers,
            ApplicationDbContext db,
            CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(rootEntityId))
                return Results.Problem("rootEntityId is required", statusCode: 400);
            try
            {
                var instance = await resolver.ResolveAsync(chainCode, rootEntityId, ct);
                var timeline = await composer.ComposeAsync(instance, ct);

                var steps = JsonSerializer.Deserialize<ChainStep[]>(instance.Definition.StepsJson, _jsonOptions)
                    ?? Array.Empty<ChainStep>();
                var rootStep = steps.FirstOrDefault(s => s.Role == ChainStep.RoleRoot);
                string? label = null;
                if (rootStep is not null)
                {
                    var labeler = labelers.FirstOrDefault(l => l.EntityType == rootStep.Entity);
                    if (labeler is not null)
                        label = await labeler.GetLabelAsync(db, rootEntityId, ct);
                }

                return Results.Ok(new TimelinePayloadDto(
                    Chain: new ChainDescriptorDto(instance.Definition.Code, instance.Definition.Name, instance.Definition.Description),
                    RootEntityId: rootEntityId,
                    RootLabel: label,
                    Truncated: timeline.Truncated,
                    Events: timeline.Events.Select(e => e.ToDto()).ToList()));
            }
            catch (ChainDefinitionNotFoundException) { return Results.NotFound(); }
            catch (ChainStepNotSupportedException ex) { return Results.Problem(ex.Message, statusCode: 500); }
        }).WithName("GetTimeline");

        // GET /api/processes/chains/recent
        g.MapGet("/chains/recent", async (
            IProcessChainResolver resolver,
            IEnumerable<IRootEntityLabeler> labelers,
            ApplicationDbContext db,
            [FromQuery] string? chainCode,
            [FromQuery] string? owner,
            [FromQuery] DateTime? since,
            [FromQuery] DateTime? until,
            [FromQuery] int limit,
            CancellationToken ct) =>
        {
            var q = new ChainQuery(
                ChainCode: chainCode,
                Owner: owner,
                Since: since,
                Until: until,
                Limit: limit <= 0 ? 100 : Math.Clamp(limit, 1, 500));
            var summaries = await resolver.RecentAsync(q, ct);

            // Enrich summaries with RootLabel by looking up each chain's root entity type.
            var chainRoots = await db.ProcessChainDefinitions.AsNoTracking()
                .Where(c => c.IsActive)
                .Select(c => new { c.Code, c.StepsJson })
                .ToListAsync(ct);
            var rootByChain = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var c in chainRoots)
            {
                var steps = JsonSerializer.Deserialize<ChainStep[]>(c.StepsJson, _jsonOptions) ?? Array.Empty<ChainStep>();
                var root = steps.FirstOrDefault(s => s.Role == ChainStep.RoleRoot);
                if (root != null) rootByChain[c.Code] = root.Entity;
            }

            var result = new List<ChainInstanceSummaryDto>(summaries.Count);
            foreach (var s in summaries)
            {
                string? label = null;
                if (rootByChain.TryGetValue(s.ChainCode, out var rootEntity))
                {
                    var labeler = labelers.FirstOrDefault(l => l.EntityType == rootEntity);
                    if (labeler != null)
                        label = await labeler.GetLabelAsync(db, s.RootEntityId, ct);
                }
                var enriched = s with { RootLabel = label };
                result.Add(enriched.ToDto());
            }
            return TypedResults.Ok(result);
        }).WithName("ListRecentChains");

        return app;
    }
}
