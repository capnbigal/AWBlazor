using System.Security.Claims;
using AWBlazorApp.Data;
using AWBlazorApp.Features.Maintenance.Audit;
using AWBlazorApp.Features.Maintenance.Domain;
using AWBlazorApp.Features.Maintenance.Models;
using AWBlazorApp.Features.Maintenance.Services;
using AWBlazorApp.Infrastructure.Persistence;
using AWBlazorApp.Shared.Models;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.Maintenance.Endpoints;

public static class MaintenanceWorkOrderEndpoints
{
    public static IEndpointRouteBuilder MapMaintenanceWorkOrderEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/maintenance-work-orders")
            .WithTags("MaintenanceWorkOrders")
            .RequireAuthorization("ApiOrCookie");

        group.MapGet("/", ListAsync).WithName("ListMaintenanceWorkOrders");
        group.MapGet("/{id:int}", GetAsync).WithName("GetMaintenanceWorkOrder");
        group.MapPost("/", CreateAsync).WithName("CreateMaintenanceWorkOrder");
        group.MapPatch("/{id:int}", UpdateAsync).WithName("UpdateMaintenanceWorkOrder");
        group.MapDelete("/{id:int}", DeleteAsync).WithName("DeleteMaintenanceWorkOrder")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Admin));
        group.MapGet("/{id:int}/history", HistoryAsync).WithName("ListMaintenanceWorkOrderHistory");

        group.MapPost("/{id:int}/schedule", ScheduleAsync).WithName("ScheduleMaintenanceWorkOrder")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));
        group.MapPost("/{id:int}/start", StartAsync).WithName("StartMaintenanceWorkOrder");
        group.MapPost("/{id:int}/hold", HoldAsync).WithName("HoldMaintenanceWorkOrder");
        group.MapPost("/{id:int}/resume", ResumeAsync).WithName("ResumeMaintenanceWorkOrder");
        group.MapPost("/{id:int}/complete", CompleteAsync).WithName("CompleteMaintenanceWorkOrder");
        group.MapPost("/{id:int}/cancel", CancelAsync).WithName("CancelMaintenanceWorkOrder");

        group.MapGet("/{id:int}/tasks", ListTasksAsync).WithName("ListMaintenanceWorkOrderTasks");
        group.MapPost("/{id:int}/tasks", CreateTaskAsync).WithName("CreateMaintenanceWorkOrderTask");
        group.MapPost("/tasks/{tId:int}/complete", CompleteTaskAsync).WithName("CompleteMaintenanceWorkOrderTask");
        group.MapDelete("/tasks/{tId:int}", DeleteTaskAsync).WithName("DeleteMaintenanceWorkOrderTask");

        return app;
    }

    private static async Task<Ok<PagedResult<MaintenanceWorkOrderDto>>> ListAsync(
        ApplicationDbContext db,
        [FromQuery] int skip = 0, [FromQuery] int take = 50,
        [FromQuery] WorkOrderStatus? status = null,
        [FromQuery] int? assetId = null,
        [FromQuery] WorkOrderType? type = null,
        [FromQuery] int? assigneeBusinessEntityId = null,
        CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 500);
        var q = db.MaintenanceWorkOrders.AsNoTracking();
        if (status.HasValue) q = q.Where(x => x.Status == status.Value);
        if (assetId.HasValue) q = q.Where(x => x.AssetId == assetId.Value);
        if (type.HasValue) q = q.Where(x => x.Type == type.Value);
        if (assigneeBusinessEntityId.HasValue) q = q.Where(x => x.AssignedBusinessEntityId == assigneeBusinessEntityId.Value);
        var total = await q.CountAsync(ct);
        var rows = await q.OrderByDescending(x => x.RaisedAt)
            .Skip(skip).Take(take).Select(x => x.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<MaintenanceWorkOrderDto>(rows, total, skip, take));
    }

    private static async Task<Results<Ok<MaintenanceWorkOrderDto>, NotFound>> GetAsync(int id, ApplicationDbContext db, CancellationToken ct)
    {
        var row = await db.MaintenanceWorkOrders.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        return row is null ? TypedResults.NotFound() : TypedResults.Ok(row.ToDto());
    }

    private static async Task<Results<Created<IdResponse>, ValidationProblem>> CreateAsync(
        CreateMaintenanceWorkOrderRequest request,
        IValidator<CreateMaintenanceWorkOrderRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());
        var entity = request.ToEntity(user.Identity?.Name);
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        db.MaintenanceWorkOrders.Add(entity);
        await db.SaveChangesAsync(ct);
        db.MaintenanceWorkOrderAuditLogs.Add(MaintenanceWorkOrderAuditService.RecordCreate(entity, user.Identity?.Name));
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return TypedResults.Created($"/api/maintenance-work-orders/{entity.Id}", new IdResponse(entity.Id));
    }

    private static async Task<Results<Ok<IdResponse>, NotFound, BadRequest<string>, ValidationProblem>> UpdateAsync(
        int id, UpdateMaintenanceWorkOrderRequest request,
        IValidator<UpdateMaintenanceWorkOrderRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());
        var entity = await db.MaintenanceWorkOrders.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null) return TypedResults.NotFound();
        if (entity.Status is WorkOrderStatus.Completed or WorkOrderStatus.Cancelled)
            return TypedResults.BadRequest("Cannot edit a Completed or Cancelled work order.");
        var before = MaintenanceWorkOrderAuditService.CaptureSnapshot(entity);
        request.ApplyTo(entity);
        db.MaintenanceWorkOrderAuditLogs.Add(MaintenanceWorkOrderAuditService.RecordUpdate(before, entity, user.Identity?.Name));
        await db.SaveChangesAsync(ct);
        return TypedResults.Ok(new IdResponse(entity.Id));
    }

    private static async Task<Results<NoContent, NotFound>> DeleteAsync(
        int id, ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var entity = await db.MaintenanceWorkOrders.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null) return TypedResults.NotFound();
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        db.MaintenanceWorkOrders.Remove(entity);
        db.MaintenanceWorkOrderAuditLogs.Add(MaintenanceWorkOrderAuditService.RecordDelete(entity, user.Identity?.Name));
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return TypedResults.NoContent();
    }

    private static async Task<Ok<PagedResult<MaintenanceWorkOrderAuditLogDto>>> HistoryAsync(
        int id, ApplicationDbContext db,
        [FromQuery] int skip = 0, [FromQuery] int take = 50, CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 500);
        var q = db.MaintenanceWorkOrderAuditLogs.AsNoTracking().Where(a => a.MaintenanceWorkOrderId == id);
        var total = await q.CountAsync(ct);
        var rows = await q.OrderByDescending(a => a.ChangedDate).ThenByDescending(a => a.Id)
            .Skip(skip).Take(take).Select(a => a.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<MaintenanceWorkOrderAuditLogDto>(rows, total, skip, take));
    }

    private static async Task<Results<NoContent, NotFound, BadRequest<string>>> ScheduleAsync(
        int id, ScheduleWorkOrderRequest request, IWorkOrderService svc, ClaimsPrincipal user, CancellationToken ct)
        => await TransitionAsync(() => svc.ScheduleAsync(id, request.ScheduledFor, request.AssigneeBusinessEntityId, user.Identity?.Name, ct));

    private static async Task<Results<NoContent, NotFound, BadRequest<string>>> StartAsync(
        int id, IWorkOrderService svc, ClaimsPrincipal user, CancellationToken ct)
        => await TransitionAsync(() => svc.StartAsync(id, user.Identity?.Name, ct));

    private static async Task<Results<NoContent, NotFound, BadRequest<string>>> HoldAsync(
        int id, WorkOrderStateChangeRequest request, IWorkOrderService svc, ClaimsPrincipal user, CancellationToken ct)
        => await TransitionAsync(() => svc.HoldAsync(id, request.Reason, user.Identity?.Name, ct));

    private static async Task<Results<NoContent, NotFound, BadRequest<string>>> ResumeAsync(
        int id, IWorkOrderService svc, ClaimsPrincipal user, CancellationToken ct)
        => await TransitionAsync(() => svc.ResumeAsync(id, user.Identity?.Name, ct));

    private static async Task<Results<NoContent, NotFound, BadRequest<string>>> CompleteAsync(
        int id, CompleteWorkOrderRequest request, IWorkOrderService svc, ClaimsPrincipal user, CancellationToken ct)
        => await TransitionAsync(() => svc.CompleteAsync(id, request.CompletionNotes, request.CompletedMeterValue, user.Identity?.Name, ct));

    private static async Task<Results<NoContent, NotFound, BadRequest<string>>> CancelAsync(
        int id, WorkOrderStateChangeRequest request, IWorkOrderService svc, ClaimsPrincipal user, CancellationToken ct)
        => await TransitionAsync(() => svc.CancelAsync(id, request.Reason, user.Identity?.Name, ct));

    private static async Task<Results<NoContent, NotFound, BadRequest<string>>> TransitionAsync(Func<Task> action)
    {
        try
        {
            await action();
            return TypedResults.NoContent();
        }
        catch (KeyNotFoundException) { return TypedResults.NotFound(); }
        catch (InvalidOperationException ex) { return TypedResults.BadRequest(ex.Message); }
    }

    private static async Task<Ok<PagedResult<MaintenanceWorkOrderTaskDto>>> ListTasksAsync(
        int id, ApplicationDbContext db, CancellationToken ct = default)
    {
        var rows = await db.MaintenanceWorkOrderTasks.AsNoTracking()
            .Where(t => t.MaintenanceWorkOrderId == id)
            .OrderBy(t => t.SequenceNumber)
            .Select(t => t.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<MaintenanceWorkOrderTaskDto>(rows, rows.Count, 0, rows.Count));
    }

    private static async Task<Results<Created<IdResponse>, NotFound, ValidationProblem>> CreateTaskAsync(
        int id, CreateWorkOrderTaskRequest request,
        IValidator<CreateWorkOrderTaskRequest> validator,
        ApplicationDbContext db, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());
        var parent = await db.MaintenanceWorkOrders.AsNoTracking().FirstOrDefaultAsync(w => w.Id == id, ct);
        if (parent is null) return TypedResults.NotFound();
        request = request with { MaintenanceWorkOrderId = id };
        var entity = request.ToEntity();
        db.MaintenanceWorkOrderTasks.Add(entity);
        await db.SaveChangesAsync(ct);
        return TypedResults.Created($"/api/maintenance-work-orders/{id}/tasks/{entity.Id}", new IdResponse(entity.Id));
    }

    private static async Task<Results<Ok<MaintenanceWorkOrderTaskDto>, NotFound>> CompleteTaskAsync(
        int tId, CompleteWorkOrderTaskRequest request,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var entity = await db.MaintenanceWorkOrderTasks.FirstOrDefaultAsync(t => t.Id == tId, ct);
        if (entity is null) return TypedResults.NotFound();
        entity.IsComplete = true;
        entity.CompletedAt = DateTime.UtcNow;
        entity.CompletedByUserId = user.Identity?.Name;
        entity.ActualMinutes = request.ActualMinutes;
        entity.SignoffNotes = request.SignoffNotes?.Trim();
        entity.ModifiedDate = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return TypedResults.Ok(entity.ToDto());
    }

    private static async Task<Results<NoContent, NotFound>> DeleteTaskAsync(
        int tId, ApplicationDbContext db, CancellationToken ct)
    {
        var entity = await db.MaintenanceWorkOrderTasks.FirstOrDefaultAsync(t => t.Id == tId, ct);
        if (entity is null) return TypedResults.NotFound();
        db.MaintenanceWorkOrderTasks.Remove(entity);
        await db.SaveChangesAsync(ct);
        return TypedResults.NoContent();
    }
}
