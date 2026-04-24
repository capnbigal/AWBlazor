using AWBlazorApp.Features.Scheduling.DeliverySchedules.Dtos;
using AWBlazorApp.Infrastructure.Persistence;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.Scheduling.DeliverySchedules.Api;

public static class DeliveryScheduleEndpoints
{
    public static IEndpointRouteBuilder MapDeliveryScheduleEndpoints(this IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/api/scheduling")
            .WithTags("Scheduling.Delivery")
            .RequireAuthorization("ApiOrCookie");

        g.MapGet("/delivery", async (
            ApplicationDbContext db,
            [FromQuery] int weekId, [FromQuery] short locationId,
            CancellationToken ct) =>
        {
            var rows = await db.CurrentDeliverySchedule.AsNoTracking()
                .Where(r => r.WeekId == weekId && r.LocationId == locationId)
                .ToListAsync(ct);
            var ordered = rows
                .OrderBy(r => r.CurrentSequence ?? int.MaxValue)
                .Select(r => r.ToDto())
                .ToList();
            return TypedResults.Ok(ordered);
        }).WithName("GetDeliverySchedule").WithSummary("Read the live vw_CurrentDeliverySchedule view for a line/week.");

        g.MapPost("/exceptions", async (
            [FromBody] CreateSchedulingExceptionRequest req,
            IValidator<CreateSchedulingExceptionRequest> validator,
            ApplicationDbContext db, HttpContext http, CancellationToken ct) =>
        {
            var v = await validator.ValidateAsync(req, ct);
            if (!v.IsValid) return Results.ValidationProblem(v.ToDictionary());

            var user = http.User?.Identity?.Name ?? "api";
            var entity = req.ToEntity(user);
            db.SchedulingExceptions.Add(entity);
            await db.SaveChangesAsync(ct);
            return Results.Created($"/api/scheduling/exceptions/{entity.Id}", entity.ToDto());
        }).WithName("CreateSchedulingException");

        g.MapPost("/exceptions/{id:int}/resolve", async (
            int id, ApplicationDbContext db, HttpContext http, CancellationToken ct) =>
        {
            var ex = await db.SchedulingExceptions.SingleOrDefaultAsync(e => e.Id == id, ct);
            if (ex is null) return Results.NotFound();
            if (ex.ResolvedAt is not null) return Results.Ok(ex.ToDto());
            ex.ResolvedAt = DateTime.UtcNow;
            ex.ResolvedBy = http.User?.Identity?.Name ?? "api";
            await db.SaveChangesAsync(ct);
            return Results.Ok(ex.ToDto());
        }).WithName("ResolveSchedulingException");

        g.MapGet("/exceptions", async (
            ApplicationDbContext db,
            [FromQuery] int? weekId, [FromQuery] short? locationId, [FromQuery] bool activeOnly = true,
            CancellationToken ct = default) =>
        {
            var q = db.SchedulingExceptions.AsNoTracking();
            if (weekId.HasValue) q = q.Where(e => e.WeekId == weekId.Value);
            if (locationId.HasValue) q = q.Where(e => e.LocationId == locationId.Value);
            if (activeOnly) q = q.Where(e => e.ResolvedAt == null);
            var list = await q.OrderByDescending(e => e.CreatedAt)
                .Select(e => e.ToDto()).ToListAsync(ct);
            return TypedResults.Ok(list);
        }).WithName("ListSchedulingExceptions");

        return app;
    }
}
