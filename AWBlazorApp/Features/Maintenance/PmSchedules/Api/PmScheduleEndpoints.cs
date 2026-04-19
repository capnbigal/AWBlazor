using System.Security.Claims;
using AWBlazorApp.Features.Identity.Domain; using AWBlazorApp.Features.Admin.Permissions.Domain;
using AWBlazorApp.Features.Maintenance.Audit;
using AWBlazorApp.Features.Maintenance.AssetProfiles.Dtos; using AWBlazorApp.Features.Maintenance.PmSchedules.Dtos; using AWBlazorApp.Features.Maintenance.SpareParts.Dtos; using AWBlazorApp.Features.Maintenance.WorkOrders.Dtos; 
using AWBlazorApp.Features.Maintenance.PmSchedules.Application.Services; using AWBlazorApp.Features.Maintenance.WorkOrders.Application.Services; 
using AWBlazorApp.Infrastructure.Persistence;
using AWBlazorApp.Shared.Dtos;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.Maintenance.PmSchedules.Api;

public static class PmScheduleEndpoints
{
    public static IEndpointRouteBuilder MapPmScheduleEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/pm-schedules")
            .WithTags("PmSchedules")
            .RequireAuthorization("ApiOrCookie");

        group.MapGet("/", ListAsync).WithName("ListPmSchedules");
        group.MapGet("/{id:int}", GetAsync).WithName("GetPmSchedule");
        group.MapPost("/", CreateAsync).WithName("CreatePmSchedule")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));
        group.MapPatch("/{id:int}", UpdateAsync).WithName("UpdatePmSchedule")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));
        group.MapDelete("/{id:int}", DeleteAsync).WithName("DeletePmSchedule")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Admin));
        group.MapGet("/{id:int}/history", HistoryAsync).WithName("ListPmScheduleHistory");

        group.MapGet("/{id:int}/tasks", ListTasksAsync).WithName("ListPmScheduleTasks");
        group.MapPost("/{id:int}/tasks", CreateTaskAsync).WithName("CreatePmScheduleTask")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));
        group.MapDelete("/tasks/{tId:int}", DeleteTaskAsync).WithName("DeletePmScheduleTask")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));

        group.MapPost("/generate-due", GenerateDueAsync).WithName("GenerateDuePmWorkOrders")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));
        group.MapPost("/{id:int}/generate-due", GenerateDueForScheduleAsync).WithName("GenerateDuePmWorkOrderForSchedule")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));

        return app;
    }

    private static async Task<Ok<PagedResult<PmScheduleDto>>> ListAsync(
        ApplicationDbContext db,
        [FromQuery] int skip = 0, [FromQuery] int take = 50,
        [FromQuery] int? assetId = null, [FromQuery] bool? isActive = null,
        CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 500);
        var q = db.PmSchedules.AsNoTracking();
        if (assetId.HasValue) q = q.Where(x => x.AssetId == assetId.Value);
        if (isActive.HasValue) q = q.Where(x => x.IsActive == isActive.Value);
        var total = await q.CountAsync(ct);
        var rows = await q.OrderBy(x => x.Code)
            .Skip(skip).Take(take).Select(x => x.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<PmScheduleDto>(rows, total, skip, take));
    }

    private static async Task<Results<Ok<PmScheduleDto>, NotFound>> GetAsync(int id, ApplicationDbContext db, CancellationToken ct)
    {
        var row = await db.PmSchedules.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        return row is null ? TypedResults.NotFound() : TypedResults.Ok(row.ToDto());
    }

    private static async Task<Results<Created<IdResponse>, ValidationProblem>> CreateAsync(
        CreatePmScheduleRequest request,
        IValidator<CreatePmScheduleRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());
        var entity = request.ToEntity();
        await db.AddWithAuditAsync(entity, e => PmScheduleAuditService.RecordCreate(e, user.Identity?.Name), ct);
        return TypedResults.Created($"/api/pm-schedules/{entity.Id}", new IdResponse(entity.Id));
    }

    private static async Task<Results<Ok<IdResponse>, NotFound, ValidationProblem>> UpdateAsync(
        int id, UpdatePmScheduleRequest request,
        IValidator<UpdatePmScheduleRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());
        var entity = await db.PmSchedules.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null) return TypedResults.NotFound();
        var before = PmScheduleAuditService.CaptureSnapshot(entity);
        request.ApplyTo(entity);
        db.PmScheduleAuditLogs.Add(PmScheduleAuditService.RecordUpdate(before, entity, user.Identity?.Name));
        await db.SaveChangesAsync(ct);
        return TypedResults.Ok(new IdResponse(entity.Id));
    }

    private static async Task<Results<NoContent, NotFound>> DeleteAsync(
        int id, ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var entity = await db.PmSchedules.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null) return TypedResults.NotFound();
        await db.DeleteWithAuditAsync(entity, PmScheduleAuditService.RecordDelete(entity, user.Identity?.Name), ct);
        return TypedResults.NoContent();
    }

    private static async Task<Ok<PagedResult<PmScheduleAuditLogDto>>> HistoryAsync(
        int id, ApplicationDbContext db,
        [FromQuery] int skip = 0, [FromQuery] int take = 50, CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 500);
        var q = db.PmScheduleAuditLogs.AsNoTracking().Where(a => a.PmScheduleId == id);
        var total = await q.CountAsync(ct);
        var rows = await q.OrderByDescending(a => a.ChangedDate).ThenByDescending(a => a.Id)
            .Skip(skip).Take(take).Select(a => a.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<PmScheduleAuditLogDto>(rows, total, skip, take));
    }

    private static async Task<Ok<PagedResult<PmScheduleTaskDto>>> ListTasksAsync(
        int id, ApplicationDbContext db, CancellationToken ct = default)
    {
        var rows = await db.PmScheduleTasks.AsNoTracking()
            .Where(t => t.PmScheduleId == id)
            .OrderBy(t => t.SequenceNumber)
            .Select(t => t.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<PmScheduleTaskDto>(rows, rows.Count, 0, rows.Count));
    }

    private static async Task<Results<Created<IdResponse>, NotFound, ValidationProblem>> CreateTaskAsync(
        int id, CreatePmScheduleTaskRequest request,
        IValidator<CreatePmScheduleTaskRequest> validator,
        ApplicationDbContext db, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());
        var parent = await db.PmSchedules.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id, ct);
        if (parent is null) return TypedResults.NotFound();
        request = request with { PmScheduleId = id };
        var entity = request.ToEntity();
        db.PmScheduleTasks.Add(entity);
        await db.SaveChangesAsync(ct);
        return TypedResults.Created($"/api/pm-schedules/{id}/tasks/{entity.Id}", new IdResponse(entity.Id));
    }

    private static async Task<Results<NoContent, NotFound>> DeleteTaskAsync(
        int tId, ApplicationDbContext db, CancellationToken ct)
    {
        var entity = await db.PmScheduleTasks.FirstOrDefaultAsync(t => t.Id == tId, ct);
        if (entity is null) return TypedResults.NotFound();
        db.PmScheduleTasks.Remove(entity);
        await db.SaveChangesAsync(ct);
        return TypedResults.NoContent();
    }

    private static async Task<Ok<GenerateDueWorkOrdersResponse>> GenerateDueAsync(
        IPmScheduleService svc, ClaimsPrincipal user, CancellationToken ct)
    {
        var count = await svc.GenerateDueWorkOrdersAsync(pmScheduleId: null, user.Identity?.Name, ct);
        return TypedResults.Ok(new GenerateDueWorkOrdersResponse(count));
    }

    private static async Task<Ok<GenerateDueWorkOrdersResponse>> GenerateDueForScheduleAsync(
        int id, IPmScheduleService svc, ClaimsPrincipal user, CancellationToken ct)
    {
        var count = await svc.GenerateDueWorkOrdersAsync(pmScheduleId: id, user.Identity?.Name, ct);
        return TypedResults.Ok(new GenerateDueWorkOrdersResponse(count));
    }
}
