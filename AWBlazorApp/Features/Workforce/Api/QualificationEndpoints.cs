using System.Security.Claims;
using AWBlazorApp.Features.Identity.Domain; using AWBlazorApp.Features.Admin.Permissions.Domain;
using AWBlazorApp.Features.Workforce.Audit;
using AWBlazorApp.Features.Workforce.Domain;
using AWBlazorApp.Features.Workforce.Dtos;
using AWBlazorApp.Features.Workforce.Services;
using AWBlazorApp.Infrastructure.Persistence;
using AWBlazorApp.Shared.Dtos;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.Workforce.Api;

public static class QualificationEndpoints
{
    public static IEndpointRouteBuilder MapQualificationEndpoints(this IEndpointRouteBuilder app)
    {
        var quals = app.MapGroup("/api/qualifications")
            .WithTags("Qualifications")
            .RequireAuthorization("ApiOrCookie");

        quals.MapGet("/", ListAsync).WithName("ListQualifications");
        quals.MapGet("/{id:int}", GetAsync).WithName("GetQualification");
        quals.MapPost("/", CreateAsync).WithName("CreateQualification")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));
        quals.MapPatch("/{id:int}", UpdateAsync).WithName("UpdateQualification")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));
        quals.MapDelete("/{id:int}", DeleteAsync).WithName("DeleteQualification")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Admin));
        quals.MapGet("/{id:int}/history", HistoryAsync).WithName("ListQualificationHistory");

        var emp = app.MapGroup("/api/employee-qualifications")
            .WithTags("EmployeeQualifications")
            .RequireAuthorization("ApiOrCookie");

        emp.MapGet("/", ListEmpAsync).WithName("ListEmployeeQualifications");
        emp.MapGet("/{id:int}", GetEmpAsync).WithName("GetEmployeeQualification");
        emp.MapPost("/", GrantEmpAsync).WithName("GrantEmployeeQualification")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));
        emp.MapDelete("/{id:int}", RevokeEmpAsync).WithName("RevokeEmployeeQualification")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));
        emp.MapGet("/{id:int}/history", EmpHistoryAsync).WithName("ListEmployeeQualificationHistory");

        var station = app.MapGroup("/api/station-qualifications")
            .WithTags("StationQualifications")
            .RequireAuthorization("ApiOrCookie");

        station.MapGet("/", ListStationAsync).WithName("ListStationQualifications");
        station.MapPost("/", CreateStationAsync).WithName("CreateStationQualification")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));
        station.MapPatch("/{id:int}", UpdateStationAsync).WithName("UpdateStationQualification")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));
        station.MapDelete("/{id:int}", DeleteStationAsync).WithName("DeleteStationQualification")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));

        var alerts = app.MapGroup("/api/qualification-alerts")
            .WithTags("QualificationAlerts")
            .RequireAuthorization("ApiOrCookie");

        alerts.MapGet("/", ListAlertsAsync).WithName("ListQualificationAlerts");
        alerts.MapGet("/{id:long}", GetAlertAsync).WithName("GetQualificationAlert");
        alerts.MapPatch("/{id:long}", AcknowledgeAlertAsync).WithName("AcknowledgeQualificationAlert")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));

        return app;
    }

    private static async Task<Ok<PagedResult<QualificationDto>>> ListAsync(
        ApplicationDbContext db,
        [FromQuery] int skip = 0, [FromQuery] int take = 50,
        [FromQuery] QualificationCategory? category = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 500);
        var q = db.Qualifications.AsNoTracking();
        if (category.HasValue) q = q.Where(x => x.Category == category.Value);
        if (isActive.HasValue) q = q.Where(x => x.IsActive == isActive.Value);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            q = q.Where(x => x.Code.Contains(s) || x.Name.Contains(s));
        }
        var total = await q.CountAsync(ct);
        var rows = await q.OrderBy(x => x.Code)
            .Skip(skip).Take(take).Select(x => x.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<QualificationDto>(rows, total, skip, take));
    }

    private static async Task<Results<Ok<QualificationDto>, NotFound>> GetAsync(
        int id, ApplicationDbContext db, CancellationToken ct)
    {
        var row = await db.Qualifications.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        return row is null ? TypedResults.NotFound() : TypedResults.Ok(row.ToDto());
    }

    private static async Task<Results<Created<IdResponse>, ValidationProblem>> CreateAsync(
        CreateQualificationRequest request,
        IValidator<CreateQualificationRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());
        var entity = request.ToEntity();
        await db.AddWithAuditAsync(entity, e => QualificationAuditService.RecordCreate(e, user.Identity?.Name), ct);
        return TypedResults.Created($"/api/qualifications/{entity.Id}", new IdResponse(entity.Id));
    }

    private static async Task<Results<Ok<IdResponse>, NotFound, ValidationProblem>> UpdateAsync(
        int id, UpdateQualificationRequest request,
        IValidator<UpdateQualificationRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());
        var entity = await db.Qualifications.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null) return TypedResults.NotFound();
        var before = QualificationAuditService.CaptureSnapshot(entity);
        request.ApplyTo(entity);
        db.QualificationAuditLogs.Add(QualificationAuditService.RecordUpdate(before, entity, user.Identity?.Name));
        await db.SaveChangesAsync(ct);
        return TypedResults.Ok(new IdResponse(entity.Id));
    }

    private static async Task<Results<NoContent, NotFound>> DeleteAsync(
        int id, ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var entity = await db.Qualifications.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null) return TypedResults.NotFound();
        await db.DeleteWithAuditAsync(entity, QualificationAuditService.RecordDelete(entity, user.Identity?.Name), ct);
        return TypedResults.NoContent();
    }

    private static async Task<Ok<PagedResult<QualificationAuditLogDto>>> HistoryAsync(
        int id, ApplicationDbContext db,
        [FromQuery] int skip = 0, [FromQuery] int take = 50, CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 500);
        var q = db.QualificationAuditLogs.AsNoTracking().Where(a => a.QualificationId == id);
        var total = await q.CountAsync(ct);
        var rows = await q.OrderByDescending(a => a.ChangedDate).ThenByDescending(a => a.Id)
            .Skip(skip).Take(take).Select(a => a.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<QualificationAuditLogDto>(rows, total, skip, take));
    }

    private static async Task<Ok<PagedResult<EmployeeQualificationDto>>> ListEmpAsync(
        ApplicationDbContext db,
        [FromQuery] int skip = 0, [FromQuery] int take = 50,
        [FromQuery] int? businessEntityId = null,
        [FromQuery] int? qualificationId = null,
        [FromQuery] bool? expiringOnly = null,
        CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 500);
        var q = db.EmployeeQualifications.AsNoTracking();
        if (businessEntityId.HasValue) q = q.Where(x => x.BusinessEntityId == businessEntityId.Value);
        if (qualificationId.HasValue) q = q.Where(x => x.QualificationId == qualificationId.Value);
        if (expiringOnly == true)
        {
            var cutoff = DateTime.UtcNow.AddDays(30);
            q = q.Where(x => x.ExpiresOn != null && x.ExpiresOn <= cutoff);
        }
        var total = await q.CountAsync(ct);
        var rows = await q.OrderByDescending(x => x.EarnedDate)
            .Skip(skip).Take(take).Select(x => x.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<EmployeeQualificationDto>(rows, total, skip, take));
    }

    private static async Task<Results<Ok<EmployeeQualificationDto>, NotFound>> GetEmpAsync(
        int id, ApplicationDbContext db, CancellationToken ct)
    {
        var row = await db.EmployeeQualifications.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        return row is null ? TypedResults.NotFound() : TypedResults.Ok(row.ToDto());
    }

    private static async Task<Results<Created<IdResponse>, ValidationProblem>> GrantEmpAsync(
        GrantEmployeeQualificationRequest request,
        IValidator<GrantEmployeeQualificationRequest> validator,
        IQualificationService qualifications,
        ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());
        var id = await qualifications.GrantAsync(
            request.BusinessEntityId, request.QualificationId,
            request.EarnedDate ?? DateTime.UtcNow, request.ExpiresOn,
            request.EvidenceUrl, request.Notes,
            user.Identity?.Name, ct);
        return TypedResults.Created($"/api/employee-qualifications/{id}", new IdResponse(id));
    }

    private static async Task<Results<NoContent, NotFound>> RevokeEmpAsync(
        int id, IQualificationService qualifications, ApplicationDbContext db,
        ClaimsPrincipal user, CancellationToken ct)
    {
        var exists = await db.EmployeeQualifications.AsNoTracking().AnyAsync(x => x.Id == id, ct);
        if (!exists) return TypedResults.NotFound();
        await qualifications.RevokeAsync(id, user.Identity?.Name, ct);
        return TypedResults.NoContent();
    }

    private static async Task<Ok<PagedResult<EmployeeQualificationAuditLogDto>>> EmpHistoryAsync(
        int id, ApplicationDbContext db,
        [FromQuery] int skip = 0, [FromQuery] int take = 50, CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 500);
        var q = db.EmployeeQualificationAuditLogs.AsNoTracking().Where(a => a.EmployeeQualificationId == id);
        var total = await q.CountAsync(ct);
        var rows = await q.OrderByDescending(a => a.ChangedDate).ThenByDescending(a => a.Id)
            .Skip(skip).Take(take).Select(a => a.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<EmployeeQualificationAuditLogDto>(rows, total, skip, take));
    }

    private static async Task<Ok<PagedResult<StationQualificationDto>>> ListStationAsync(
        ApplicationDbContext db,
        [FromQuery] int skip = 0, [FromQuery] int take = 100,
        [FromQuery] int? stationId = null,
        [FromQuery] int? qualificationId = null,
        CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 500);
        var q = db.StationQualifications.AsNoTracking();
        if (stationId.HasValue) q = q.Where(x => x.StationId == stationId.Value);
        if (qualificationId.HasValue) q = q.Where(x => x.QualificationId == qualificationId.Value);
        var total = await q.CountAsync(ct);
        var rows = await q.OrderBy(x => x.StationId).ThenBy(x => x.QualificationId)
            .Skip(skip).Take(take).Select(x => x.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<StationQualificationDto>(rows, total, skip, take));
    }

    private static async Task<Results<Created<IdResponse>, Conflict<string>, ValidationProblem>> CreateStationAsync(
        CreateStationQualificationRequest request,
        IValidator<CreateStationQualificationRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());
        var exists = await db.StationQualifications.AnyAsync(
            x => x.StationId == request.StationId && x.QualificationId == request.QualificationId, ct);
        if (exists) return TypedResults.Conflict("Station already has this qualification.");
        var entity = request.ToEntity();
        db.StationQualifications.Add(entity);
        await db.SaveChangesAsync(ct);
        db.StationQualificationAuditLogs.Add(StationQualificationAuditService.RecordCreate(entity, user.Identity?.Name));
        await db.SaveChangesAsync(ct);
        return TypedResults.Created($"/api/station-qualifications/{entity.Id}", new IdResponse(entity.Id));
    }

    private static async Task<Results<Ok<IdResponse>, NotFound>> UpdateStationAsync(
        int id, UpdateStationQualificationRequest request,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var entity = await db.StationQualifications.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null) return TypedResults.NotFound();
        request.ApplyTo(entity);
        db.StationQualificationAuditLogs.Add(StationQualificationAuditService.RecordUpdate(entity, user.Identity?.Name));
        await db.SaveChangesAsync(ct);
        return TypedResults.Ok(new IdResponse(entity.Id));
    }

    private static async Task<Results<NoContent, NotFound>> DeleteStationAsync(
        int id, ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var entity = await db.StationQualifications.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null) return TypedResults.NotFound();
        db.StationQualifications.Remove(entity);
        db.StationQualificationAuditLogs.Add(StationQualificationAuditService.RecordDelete(entity, user.Identity?.Name));
        await db.SaveChangesAsync(ct);
        return TypedResults.NoContent();
    }

    private static async Task<Ok<PagedResult<QualificationAlertDto>>> ListAlertsAsync(
        ApplicationDbContext db,
        [FromQuery] int skip = 0, [FromQuery] int take = 50,
        [FromQuery] QualificationAlertStatus? status = null,
        [FromQuery] int? businessEntityId = null,
        [FromQuery] int? stationId = null,
        CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 500);
        var q = db.QualificationAlerts.AsNoTracking();
        if (status.HasValue) q = q.Where(a => a.Status == status.Value);
        if (businessEntityId.HasValue) q = q.Where(a => a.BusinessEntityId == businessEntityId.Value);
        if (stationId.HasValue) q = q.Where(a => a.StationId == stationId.Value);
        var total = await q.CountAsync(ct);
        var rows = await q.OrderByDescending(a => a.RaisedAt)
            .Skip(skip).Take(take).Select(a => a.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<QualificationAlertDto>(rows, total, skip, take));
    }

    private static async Task<Results<Ok<QualificationAlertDto>, NotFound>> GetAlertAsync(
        long id, ApplicationDbContext db, CancellationToken ct)
    {
        var row = await db.QualificationAlerts.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id, ct);
        return row is null ? TypedResults.NotFound() : TypedResults.Ok(row.ToDto());
    }

    private static async Task<Results<Ok<QualificationAlertDto>, NotFound>> AcknowledgeAlertAsync(
        long id, AcknowledgeQualificationAlertRequest request,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var entity = await db.QualificationAlerts.FirstOrDefaultAsync(a => a.Id == id, ct);
        if (entity is null) return TypedResults.NotFound();
        var before = QualificationAlertAuditService.CaptureSnapshot(entity);
        entity.Status = request.TargetStatus;
        entity.AcknowledgedAt = DateTime.UtcNow;
        entity.AcknowledgedByUserId = user.Identity?.Name;
        if (!string.IsNullOrWhiteSpace(request.Notes)) entity.Notes = request.Notes.Trim();
        entity.ModifiedDate = DateTime.UtcNow;
        db.QualificationAlertAuditLogs.Add(QualificationAlertAuditService.RecordUpdate(before, entity, user.Identity?.Name));
        await db.SaveChangesAsync(ct);
        return TypedResults.Ok(entity.ToDto());
    }
}
