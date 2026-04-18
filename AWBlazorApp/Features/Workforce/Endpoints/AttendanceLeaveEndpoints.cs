using System.Security.Claims;
using AWBlazorApp.Data;
using AWBlazorApp.Features.Workforce.Audit;
using AWBlazorApp.Features.Workforce.Domain;
using AWBlazorApp.Features.Workforce.Models;
using AWBlazorApp.Features.Workforce.Services;
using AWBlazorApp.Infrastructure.Persistence;
using AWBlazorApp.Shared.Models;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.Workforce.Endpoints;

public static class AttendanceLeaveEndpoints
{
    public static IEndpointRouteBuilder MapAttendanceLeaveEndpoints(this IEndpointRouteBuilder app)
    {
        var att = app.MapGroup("/api/attendance-events")
            .WithTags("AttendanceEvents")
            .RequireAuthorization("ApiOrCookie");

        att.MapGet("/", ListAttendanceAsync).WithName("ListAttendanceEvents");
        att.MapGet("/{id:long}", GetAttendanceAsync).WithName("GetAttendanceEvent");
        att.MapPost("/", CreateAttendanceAsync).WithName("CreateAttendanceEvent")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));
        att.MapPatch("/{id:long}", UpdateAttendanceAsync).WithName("UpdateAttendanceEvent")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));
        att.MapDelete("/{id:long}", DeleteAttendanceAsync).WithName("DeleteAttendanceEvent")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Admin));

        var leave = app.MapGroup("/api/leave-requests")
            .WithTags("LeaveRequests")
            .RequireAuthorization("ApiOrCookie");

        leave.MapGet("/", ListLeaveAsync).WithName("ListLeaveRequests");
        leave.MapGet("/{id:int}", GetLeaveAsync).WithName("GetLeaveRequest");
        leave.MapPost("/", CreateLeaveAsync).WithName("CreateLeaveRequest");
        leave.MapPost("/{id:int}/approve", ApproveLeaveAsync).WithName("ApproveLeaveRequest")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));
        leave.MapPost("/{id:int}/reject", RejectLeaveAsync).WithName("RejectLeaveRequest")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));
        leave.MapPost("/{id:int}/cancel", CancelLeaveAsync).WithName("CancelLeaveRequest");
        leave.MapGet("/{id:int}/history", LeaveHistoryAsync).WithName("ListLeaveRequestHistory");

        return app;
    }

    private static async Task<Ok<PagedResult<AttendanceEventDto>>> ListAttendanceAsync(
        ApplicationDbContext db,
        [FromQuery] int skip = 0, [FromQuery] int take = 50,
        [FromQuery] int? businessEntityId = null,
        [FromQuery] DateOnly? from = null,
        [FromQuery] DateOnly? to = null,
        [FromQuery] AttendanceStatus? status = null,
        CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 500);
        var q = db.AttendanceEvents.AsNoTracking();
        if (businessEntityId.HasValue) q = q.Where(x => x.BusinessEntityId == businessEntityId.Value);
        if (from.HasValue) q = q.Where(x => x.ShiftDate >= from.Value);
        if (to.HasValue) q = q.Where(x => x.ShiftDate <= to.Value);
        if (status.HasValue) q = q.Where(x => x.Status == status.Value);
        var total = await q.CountAsync(ct);
        var rows = await q.OrderByDescending(x => x.ShiftDate).ThenByDescending(x => x.Id)
            .Skip(skip).Take(take).Select(x => x.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<AttendanceEventDto>(rows, total, skip, take));
    }

    private static async Task<Results<Ok<AttendanceEventDto>, NotFound>> GetAttendanceAsync(
        long id, ApplicationDbContext db, CancellationToken ct)
    {
        var row = await db.AttendanceEvents.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        return row is null ? TypedResults.NotFound() : TypedResults.Ok(row.ToDto());
    }

    private static async Task<Results<Created<IdResponse>, ValidationProblem>> CreateAttendanceAsync(
        CreateAttendanceEventRequest request,
        IValidator<CreateAttendanceEventRequest> validator,
        ApplicationDbContext db, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());
        var entity = request.ToEntity();
        db.AttendanceEvents.Add(entity);
        await db.SaveChangesAsync(ct);
        return TypedResults.Created($"/api/attendance-events/{entity.Id}", new IdResponse((int)entity.Id));
    }

    private static async Task<Results<Ok<AttendanceEventDto>, NotFound>> UpdateAttendanceAsync(
        long id, UpdateAttendanceEventRequest request,
        ApplicationDbContext db, CancellationToken ct)
    {
        var entity = await db.AttendanceEvents.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null) return TypedResults.NotFound();
        request.ApplyTo(entity);
        await db.SaveChangesAsync(ct);
        return TypedResults.Ok(entity.ToDto());
    }

    private static async Task<Results<NoContent, NotFound>> DeleteAttendanceAsync(
        long id, ApplicationDbContext db, CancellationToken ct)
    {
        var entity = await db.AttendanceEvents.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null) return TypedResults.NotFound();
        db.AttendanceEvents.Remove(entity);
        await db.SaveChangesAsync(ct);
        return TypedResults.NoContent();
    }

    private static async Task<Ok<PagedResult<LeaveRequestDto>>> ListLeaveAsync(
        ApplicationDbContext db,
        [FromQuery] int skip = 0, [FromQuery] int take = 50,
        [FromQuery] int? businessEntityId = null,
        [FromQuery] LeaveStatus? status = null,
        [FromQuery] LeaveType? leaveType = null,
        CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 500);
        var q = db.LeaveRequests.AsNoTracking();
        if (businessEntityId.HasValue) q = q.Where(x => x.BusinessEntityId == businessEntityId.Value);
        if (status.HasValue) q = q.Where(x => x.Status == status.Value);
        if (leaveType.HasValue) q = q.Where(x => x.LeaveType == leaveType.Value);
        var total = await q.CountAsync(ct);
        var rows = await q.OrderByDescending(x => x.RequestedAt)
            .Skip(skip).Take(take).Select(x => x.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<LeaveRequestDto>(rows, total, skip, take));
    }

    private static async Task<Results<Ok<LeaveRequestDto>, NotFound>> GetLeaveAsync(
        int id, ApplicationDbContext db, CancellationToken ct)
    {
        var row = await db.LeaveRequests.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        return row is null ? TypedResults.NotFound() : TypedResults.Ok(row.ToDto());
    }

    private static async Task<Results<Created<IdResponse>, ValidationProblem>> CreateLeaveAsync(
        CreateLeaveRequestRequest request,
        IValidator<CreateLeaveRequestRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());
        var entity = request.ToEntity(user.Identity?.Name);
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        db.LeaveRequests.Add(entity);
        await db.SaveChangesAsync(ct);
        db.LeaveRequestAuditLogs.Add(LeaveRequestAuditService.RecordCreate(entity, user.Identity?.Name));
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return TypedResults.Created($"/api/leave-requests/{entity.Id}", new IdResponse(entity.Id));
    }

    private static async Task<Results<NoContent, NotFound, BadRequest<string>>> ApproveLeaveAsync(
        int id, ReviewLeaveRequestRequest request,
        ILeaveRequestService leave, ClaimsPrincipal user, CancellationToken ct)
    {
        try
        {
            await leave.ApproveAsync(id, request.Notes, user.Identity?.Name, ct);
            return TypedResults.NoContent();
        }
        catch (KeyNotFoundException)
        {
            return TypedResults.NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return TypedResults.BadRequest(ex.Message);
        }
    }

    private static async Task<Results<NoContent, NotFound, BadRequest<string>>> RejectLeaveAsync(
        int id, ReviewLeaveRequestRequest request,
        ILeaveRequestService leave, ClaimsPrincipal user, CancellationToken ct)
    {
        try
        {
            await leave.RejectAsync(id, request.Notes, user.Identity?.Name, ct);
            return TypedResults.NoContent();
        }
        catch (KeyNotFoundException)
        {
            return TypedResults.NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return TypedResults.BadRequest(ex.Message);
        }
    }

    private static async Task<Results<NoContent, NotFound, BadRequest<string>>> CancelLeaveAsync(
        int id, ILeaveRequestService leave, ClaimsPrincipal user, CancellationToken ct)
    {
        try
        {
            await leave.CancelAsync(id, user.Identity?.Name, ct);
            return TypedResults.NoContent();
        }
        catch (KeyNotFoundException)
        {
            return TypedResults.NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return TypedResults.BadRequest(ex.Message);
        }
    }

    private static async Task<Ok<PagedResult<LeaveRequestAuditLogDto>>> LeaveHistoryAsync(
        int id, ApplicationDbContext db,
        [FromQuery] int skip = 0, [FromQuery] int take = 50, CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 500);
        var q = db.LeaveRequestAuditLogs.AsNoTracking().Where(a => a.LeaveRequestId == id);
        var total = await q.CountAsync(ct);
        var rows = await q.OrderByDescending(a => a.ChangedDate).ThenByDescending(a => a.Id)
            .Skip(skip).Take(take).Select(a => a.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<LeaveRequestAuditLogDto>(rows, total, skip, take));
    }
}
