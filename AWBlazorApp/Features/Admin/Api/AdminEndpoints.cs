using AWBlazorApp.Features.Identity.Domain; using AWBlazorApp.Features.Admin.Permissions.Domain;
using AWBlazorApp.Features.Admin.Services;
using AWBlazorApp.Features.Performance.Jobs;
using AWBlazorApp.Infrastructure.Persistence;
using AWBlazorApp.Shared.Dtos;
using AWBlazorApp.Shared.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.Admin.Api;

public static class AdminEndpoints
{
    public static IEndpointRouteBuilder MapAdminEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/admin")
            .WithTags("Admin")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Admin));

        group.MapGet("/data", async Task<Ok<AdminDataResponse>> (ApplicationDbContext db, CancellationToken ct) =>
        {
            var forecasts = await db.ForecastDefinitions.CountAsync(f => f.DeletedDate == null, ct);
            var toolSlots = await db.ToolSlotConfigurations.CountAsync(ct);

            return TypedResults.Ok(new AdminDataResponse(
            [
                new PageStats("Forecasts", forecasts),
                new PageStats("ToolSlotConfigurations", toolSlots),
            ]));
        })
        .WithName("GetAdminData")
        .WithSummary("Dashboard counts for the admin landing page.");

        group.MapPost("/clear-lookup-cache", Ok<ClearLookupCacheResponse> (LookupService lookups) =>
        {
            var cleared = lookups.ClearCache();
            return TypedResults.Ok(new ClearLookupCacheResponse(cleared));
        })
        .WithName("ClearLookupCache")
        .WithSummary("Evict cached reference-data lookups so the next request reloads from the database.");

        group.MapPost("/seed-demo-data", async Task<Ok<DemoSeedResult>> (
            DemoDataSeeder seeder, CancellationToken ct) =>
        {
            var result = await seeder.SeedAllAsync(ct);
            return TypedResults.Ok(result);
        })
        .WithName("SeedDemoData")
        .WithSummary("Idempotent seed of representative demo data across M6-M9 modules. Safe to re-run — each module skips if already seeded.");

        group.MapPost("/fill-demo-data", async Task<Ok<DemoFillResult>> (
            DemoDataFiller filler,
            int? count,
            CancellationToken ct) =>
        {
            var result = await filler.FillAsync(count ?? 5, ct);
            return TypedResults.Ok(result);
        })
        .WithName("FillDemoData")
        .WithSummary("Additive — adds fresh transactional rows on every call across the new module tables (workforce attendance/leave/handover/announcements, engineering ECOs/docs/deviations, maintenance WOs/meter readings/logs, performance OEE/production-metric next-day, enterprise stations + assets). Pass ?count=N (1-50, default 5) to scale per-call volume. Skips if the seed baseline hasn't run.");

        group.MapPost("/run-metrics-rollup", async Task<Ok<MetricsRollupResult>> (
            MetricsRollupJob job,
            DateOnly? date,
            int? days,
            decimal? idealCycleSeconds,
            CancellationToken ct) =>
        {
            var result = await job.RunAsync(date, days, idealCycleSeconds, ct);
            return TypedResults.Ok(result);
        })
        .WithName("RunMetricsRollup")
        .WithSummary("Manually trigger the performance metrics rollup. Same code path as the Hangfire 'performance-metrics-rollup' recurring job. Optional parameters: ?date=yyyy-MM-dd targets a specific end date (defaults to yesterday); ?days=N (1-365) backfills a range ending on the target date and going back N-1 days, useful for a fresh deploy that needs historical OEE/production charts; ?idealCycleSeconds=N overrides the default 60s when a station doesn't carry its own IdealCycleSeconds. Maintenance monthly metrics fire for every distinct prior-month covered by the range.");

        return app;
    }
}

public sealed record ClearLookupCacheResponse(int EntriesCleared);
