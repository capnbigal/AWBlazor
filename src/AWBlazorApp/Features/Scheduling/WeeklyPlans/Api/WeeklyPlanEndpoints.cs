using AWBlazorApp.Features.Scheduling.WeeklyPlans.Application;
using AWBlazorApp.Features.Scheduling.WeeklyPlans.Domain;
using AWBlazorApp.Features.Scheduling.WeeklyPlans.Dtos;
using AWBlazorApp.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.Scheduling.WeeklyPlans.Api;

public static class WeeklyPlanEndpoints
{
    public static IEndpointRouteBuilder MapWeeklyPlanEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/scheduling/weekly-plans")
            .WithTags("Scheduling.WeeklyPlans")
            .RequireAuthorization("ApiOrCookie");

        group.MapPost("/generate", async (
            [FromBody] GenerateWeeklyPlanRequest req,
            IWeeklyPlanGenerator generator,
            HttpContext http,
            CancellationToken ct) =>
        {
            var user = http.User?.Identity?.Name ?? "api";
            try
            {
                var opts = new WeeklyPlanGenerationOptions(
                    StrictCapacity: req.StrictCapacity,
                    DryRun: req.DryRun);
                var result = await generator.GenerateAsync(req.WeekId, req.LocationId, opts, user, ct);
                return Results.Ok(result);
            }
            catch (CapacityExceededException ex) { return Results.Problem(ex.Message, statusCode: 409); }
            catch (InvalidOperationException ex) { return Results.Problem(ex.Message, statusCode: 400); }
        }).WithName("GenerateWeeklyPlan").WithSummary("Generate (or dry-run) a weekly plan for one line.");

        group.MapGet("/", async (
            ApplicationDbContext db,
            [FromQuery] int? weekId,
            [FromQuery] short? locationId,
            CancellationToken ct) =>
        {
            var q = db.WeeklyPlans.AsNoTracking();
            if (weekId.HasValue) q = q.Where(x => x.WeekId == weekId.Value);
            if (locationId.HasValue) q = q.Where(x => x.LocationId == locationId.Value);
            var list = await q.OrderByDescending(x => x.PublishedAt)
                .Select(p => p.ToDto()).ToListAsync(ct);
            return TypedResults.Ok(list);
        }).WithName("ListWeeklyPlans");

        group.MapGet("/{id:int}/items", async (
            int id, ApplicationDbContext db, CancellationToken ct) =>
        {
            var rows = await db.WeeklyPlanItems.AsNoTracking()
                .Where(i => i.WeeklyPlanId == id)
                .OrderBy(i => i.PlannedSequence)
                .Select(i => i.ToDto()).ToListAsync(ct);
            return TypedResults.Ok(rows);
        }).WithName("ListWeeklyPlanItems");

        return app;
    }
}
