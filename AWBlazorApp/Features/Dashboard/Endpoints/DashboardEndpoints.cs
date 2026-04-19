using AWBlazorApp.Features.Dashboard.Models;
using AWBlazorApp.Features.Dashboard.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace AWBlazorApp.Features.Dashboard.Endpoints;

public static class DashboardEndpoints
{
    public static IEndpointRouteBuilder MapDashboardEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/dashboard")
            .WithTags("Dashboard")
            .RequireAuthorization("ApiOrCookie");

        group.MapGet("/plant", async Task<Ok<PlantDashboardDto>> (
            IPlantDashboardService svc, CancellationToken ct) =>
        {
            var dto = await svc.GetAsync(ct);
            return TypedResults.Ok(dto);
        })
        .WithName("GetPlantDashboard")
        .WithSummary("Cross-module plant dashboard. Single payload aggregating headline KPIs, critical alerts, per-module health cards, recent activity feed, and 7-day trends across wf/eng/maint/perf/qa/mes/lgx/inv/org. Cached for 5 minutes — call /api/dashboard/plant/refresh to bust the cache.");

        group.MapPost("/plant/refresh", Ok (IPlantDashboardService svc) =>
        {
            svc.Invalidate();
            return TypedResults.Ok();
        })
        .WithName("RefreshPlantDashboard")
        .WithSummary("Evicts the cached plant dashboard so the next GET reloads from the database.");

        return app;
    }
}
