using AWBlazorApp.Features.Scheduling.Alerts.Dtos;
using AWBlazorApp.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.Scheduling.Alerts.Api;

public static class AlertEndpoints
{
    public static IEndpointRouteBuilder MapSchedulingAlertEndpoints(this IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/api/scheduling/alerts")
            .WithTags("Scheduling.Alerts")
            .RequireAuthorization("ApiOrCookie");

        g.MapGet("/", async (
            ApplicationDbContext db,
            [FromQuery] bool unacknowledgedOnly = true,
            [FromQuery] int? weekId = null,
            [FromQuery] short? locationId = null,
            CancellationToken ct = default) =>
        {
            var q = db.SchedulingAlerts.AsNoTracking();
            if (unacknowledgedOnly) q = q.Where(a => a.AcknowledgedAt == null);
            if (weekId.HasValue) q = q.Where(a => a.WeekId == weekId.Value);
            if (locationId.HasValue) q = q.Where(a => a.LocationId == locationId.Value);
            var list = await q.OrderByDescending(a => a.CreatedAt).Take(500)
                .Select(a => a.ToDto()).ToListAsync(ct);
            return TypedResults.Ok(list);
        }).WithName("ListSchedulingAlerts");

        g.MapPost("/{id:int}/acknowledge", async (
            int id, ApplicationDbContext db, HttpContext http, CancellationToken ct) =>
        {
            var a = await db.SchedulingAlerts.SingleOrDefaultAsync(x => x.Id == id, ct);
            if (a is null) return Results.NotFound();
            if (a.AcknowledgedAt is not null) return Results.Ok(a.ToDto());
            a.AcknowledgedAt = DateTime.UtcNow;
            a.AcknowledgedBy = http.User?.Identity?.Name ?? "api";
            await db.SaveChangesAsync(ct);
            return Results.Ok(a.ToDto());
        }).WithName("AcknowledgeSchedulingAlert");

        return app;
    }
}
