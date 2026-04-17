using AWBlazorApp.Data;
using AWBlazorApp.Infrastructure.Persistence;
using AWBlazorApp.Shared.Models;
using AWBlazorApp.Shared.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.Admin.Endpoints;

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

        return app;
    }
}

public sealed record ClearLookupCacheResponse(int EntriesCleared);
