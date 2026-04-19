using System.Security.Claims;
using AWBlazorApp.Data;
using AWBlazorApp.Infrastructure.Persistence;
using AWBlazorApp.Shared.Models;
using AWBlazorApp.Features.Mes.Audit;
using AWBlazorApp.Features.Mes.Domain;
using AWBlazorApp.Features.Mes.Models;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.Mes.Endpoints;

public static class ShopFloorEndpoints
{
    public static IEndpointRouteBuilder MapOperatorClockEventEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/operator-clock-events")
            .WithTags("OperatorClockEvents")
            .RequireAuthorization("ApiOrCookie");

        group.MapGet("/", async (
            ApplicationDbContext db,
            [FromQuery] int skip = 0, [FromQuery] int take = 100,
            [FromQuery] int? stationId = null, [FromQuery] int? businessEntityId = null,
            [FromQuery] bool openOnly = false,
            CancellationToken ct = default) =>
        {
            take = Math.Clamp(take, 1, 1000);
            var q = db.OperatorClockEvents.AsNoTracking();
            if (stationId.HasValue) q = q.Where(x => x.StationId == stationId.Value);
            if (businessEntityId.HasValue) q = q.Where(x => x.BusinessEntityId == businessEntityId.Value);
            if (openOnly) q = q.Where(x => x.ClockOutAt == null);
            var total = await q.CountAsync(ct);
            var rows = await q.OrderByDescending(x => x.ClockInAt).Skip(skip).Take(take).Select(x => x.ToDto()).ToListAsync(ct);
            return TypedResults.Ok(new PagedResult<OperatorClockEventDto>(rows, total, skip, take));
        }).WithName("ListOperatorClockEvents");

        group.MapPost("/", async (
            CreateOperatorClockEventRequest request,
            IValidator<CreateOperatorClockEventRequest> validator,
            ApplicationDbContext db,
            IEnumerable<AWBlazorApp.Shared.Services.IPostingTriggerHook> triggerHooks,
            ILoggerFactory loggerFactory,
            CancellationToken ct) =>
        {
            var v = await validator.ValidateAsync(request, ct);
            if (!v.IsValid) return Results.ValidationProblem(v.ToDictionary());
            var entity = request.ToEntity();
            db.OperatorClockEvents.Add(entity);
            await db.SaveChangesAsync(ct);

            // Best-effort downstream hooks (qualification check, etc.). Each hook failure is
            // caught + logged so a downstream bug never blocks a clock-in.
            var hookLogger = loggerFactory.CreateLogger("OperatorClockInHooks");
            var hookCtx = new AWBlazorApp.Shared.Services.OperatorClockedInContext(
                entity.Id, entity.StationId, entity.BusinessEntityId, entity.ProductionRunId, entity.ClockInAt);
            foreach (var hook in triggerHooks)
            {
                try { await hook.AfterOperatorClockedInAsync(hookCtx, ct); }
                catch (Exception ex) { hookLogger.LogWarning(ex, "Clock-in hook {Hook} failed for event {Id}", hook.GetType().Name, entity.Id); }
            }

            return Results.Created($"/api/operator-clock-events/{entity.Id}", new IdResponse((int)entity.Id));
        }).WithName("CreateOperatorClockEvent")
          .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));

        group.MapPost("/{id:long}/close", async (
            long id, CloseOperatorClockEventRequest request,
            ApplicationDbContext db, CancellationToken ct) =>
        {
            var entity = await db.OperatorClockEvents.FirstOrDefaultAsync(x => x.Id == id, ct);
            if (entity is null) return Results.NotFound();
            if (entity.ClockOutAt is not null) return Results.BadRequest("Already clocked out.");
            entity.ClockOutAt = request.ClockOutAt ?? DateTime.UtcNow;
            entity.Notes = request.Notes?.Trim() ?? entity.Notes;
            entity.ModifiedDate = DateTime.UtcNow;
            await db.SaveChangesAsync(ct);
            return Results.Ok(new IdResponse((int)id));
        }).WithName("CloseOperatorClockEvent")
          .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));

        return app;
    }

    public static IEndpointRouteBuilder MapDowntimeEventEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/downtime-events")
            .WithTags("DowntimeEvents")
            .RequireAuthorization("ApiOrCookie");

        group.MapGet("/", async (
            ApplicationDbContext db,
            [FromQuery] int skip = 0, [FromQuery] int take = 100,
            [FromQuery] int? stationId = null, [FromQuery] int? downtimeReasonId = null,
            [FromQuery] bool openOnly = false,
            CancellationToken ct = default) =>
        {
            take = Math.Clamp(take, 1, 1000);
            var q = db.DowntimeEvents.AsNoTracking();
            if (stationId.HasValue) q = q.Where(x => x.StationId == stationId.Value);
            if (downtimeReasonId.HasValue) q = q.Where(x => x.DowntimeReasonId == downtimeReasonId.Value);
            if (openOnly) q = q.Where(x => x.EndAt == null);
            var total = await q.CountAsync(ct);
            var rows = await q.OrderByDescending(x => x.StartAt).Skip(skip).Take(take).Select(x => x.ToDto()).ToListAsync(ct);
            return TypedResults.Ok(new PagedResult<DowntimeEventDto>(rows, total, skip, take));
        }).WithName("ListDowntimeEvents");

        group.MapPost("/", async (
            CreateDowntimeEventRequest request,
            IValidator<CreateDowntimeEventRequest> validator,
            ApplicationDbContext db, CancellationToken ct) =>
        {
            var v = await validator.ValidateAsync(request, ct);
            if (!v.IsValid) return Results.ValidationProblem(v.ToDictionary());
            var entity = request.ToEntity();
            db.DowntimeEvents.Add(entity);
            await db.SaveChangesAsync(ct);
            return Results.Created($"/api/downtime-events/{entity.Id}", new IdResponse((int)entity.Id));
        }).WithName("CreateDowntimeEvent")
          .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));

        group.MapPost("/{id:long}/close", async (
            long id, CloseDowntimeEventRequest request,
            ApplicationDbContext db, CancellationToken ct) =>
        {
            var entity = await db.DowntimeEvents.FirstOrDefaultAsync(x => x.Id == id, ct);
            if (entity is null) return Results.NotFound();
            if (entity.EndAt is not null) return Results.BadRequest("Already closed.");
            entity.EndAt = request.EndAt ?? DateTime.UtcNow;
            entity.Notes = request.Notes?.Trim() ?? entity.Notes;
            entity.ModifiedDate = DateTime.UtcNow;
            await db.SaveChangesAsync(ct);
            return Results.Ok(new IdResponse((int)id));
        }).WithName("CloseDowntimeEvent")
          .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));

        return app;
    }

    public static IEndpointRouteBuilder MapDowntimeReasonEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/downtime-reasons")
            .WithTags("DowntimeReasons")
            .RequireAuthorization("ApiOrCookie");

        group.MapGet("/", async (ApplicationDbContext db, CancellationToken ct) =>
        {
            var rows = await db.DowntimeReasons.AsNoTracking().OrderBy(r => r.Code).Select(r => r.ToDto()).ToListAsync(ct);
            return TypedResults.Ok(new PagedResult<DowntimeReasonDto>(rows, rows.Count, 0, rows.Count));
        }).WithName("ListDowntimeReasons");

        group.MapGet("/{id:int}", async (int id, ApplicationDbContext db, CancellationToken ct) =>
        {
            var row = await db.DowntimeReasons.AsNoTracking().FirstOrDefaultAsync(r => r.Id == id, ct);
            return row is null ? Results.NotFound() : Results.Ok(row.ToDto());
        }).WithName("GetDowntimeReason");

        group.MapPost("/", async (
            CreateDowntimeReasonRequest request,
            IValidator<CreateDowntimeReasonRequest> validator,
            ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct) =>
        {
            var v = await validator.ValidateAsync(request, ct);
            if (!v.IsValid) return Results.ValidationProblem(v.ToDictionary());
            var entity = request.ToEntity();
            await db.AddWithAuditAsync(entity, e => DowntimeReasonAuditService.RecordCreate(e, user.Identity?.Name), ct);
            return Results.Created($"/api/downtime-reasons/{entity.Id}", new IdResponse(entity.Id));
        }).WithName("CreateDowntimeReason")
          .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));

        group.MapPatch("/{id:int}", async (
            int id, UpdateDowntimeReasonRequest request,
            IValidator<UpdateDowntimeReasonRequest> validator,
            ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct) =>
        {
            var v = await validator.ValidateAsync(request, ct);
            if (!v.IsValid) return Results.ValidationProblem(v.ToDictionary());
            var entity = await db.DowntimeReasons.FirstOrDefaultAsync(r => r.Id == id, ct);
            if (entity is null) return Results.NotFound();
            var before = DowntimeReasonAuditService.CaptureSnapshot(entity);
            request.ApplyTo(entity);
            db.DowntimeReasonAuditLogs.Add(DowntimeReasonAuditService.RecordUpdate(before, entity, user.Identity?.Name));
            await db.SaveChangesAsync(ct);
            return Results.Ok(new IdResponse(entity.Id));
        }).WithName("UpdateDowntimeReason")
          .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));

        group.MapDelete("/{id:int}", async (
            int id, ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct) =>
        {
            var entity = await db.DowntimeReasons.FirstOrDefaultAsync(r => r.Id == id, ct);
            if (entity is null) return Results.NotFound();
            var inUse = await db.DowntimeEvents.AnyAsync(d => d.DowntimeReasonId == id, ct);
            if (inUse) return Results.BadRequest("Reason is referenced by downtime events; deactivate instead.");
            await db.DeleteWithAuditAsync(entity, DowntimeReasonAuditService.RecordDelete(entity, user.Identity?.Name), ct);
            return Results.NoContent();
        }).WithName("DeleteDowntimeReason")
          .RequireAuthorization(p => p.RequireRole(AppRoles.Admin));

        group.MapGet("/{id:int}/history", async (
            int id, ApplicationDbContext db,
            [FromQuery] int skip = 0, [FromQuery] int take = 50, CancellationToken ct = default) =>
        {
            take = Math.Clamp(take, 1, 500);
            var q = db.DowntimeReasonAuditLogs.AsNoTracking().Where(a => a.DowntimeReasonId == id);
            var total = await q.CountAsync(ct);
            var rows = await q.OrderByDescending(a => a.ChangedDate).ThenByDescending(a => a.Id)
                .Skip(skip).Take(take).Select(a => a.ToDto()).ToListAsync(ct);
            return TypedResults.Ok(new PagedResult<DowntimeReasonAuditLogDto>(rows, total, skip, take));
        }).WithName("ListDowntimeReasonHistory");

        return app;
    }
}
