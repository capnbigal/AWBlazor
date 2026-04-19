using System.Security.Claims;
using AWBlazorApp.Features.Identity.Domain; using AWBlazorApp.Features.Admin.Permissions.Domain;
using AWBlazorApp.Features.Workforce.Audit;
using AWBlazorApp.Features.Workforce.Announcements.Domain; using AWBlazorApp.Features.Workforce.Attendance.Domain; using AWBlazorApp.Features.Workforce.EmployeeQualifications.Domain; using AWBlazorApp.Features.Workforce.LeaveRequests.Domain; using AWBlazorApp.Features.Workforce.Qualifications.Domain; using AWBlazorApp.Features.Workforce.Alerts.Domain; using AWBlazorApp.Features.Workforce.HandoverNotes.Domain; using AWBlazorApp.Features.Workforce.StationQualifications.Domain; using AWBlazorApp.Features.Workforce.TrainingCourses.Domain; using AWBlazorApp.Features.Workforce.TrainingRecords.Domain; 
using AWBlazorApp.Features.Workforce.Dtos;
using AWBlazorApp.Infrastructure.Persistence;
using AWBlazorApp.Shared.Dtos;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.Workforce.Api;

public static class CommunicationEndpoints
{
    public static IEndpointRouteBuilder MapCommunicationEndpoints(this IEndpointRouteBuilder app)
    {
        var notes = app.MapGroup("/api/shift-handover-notes")
            .WithTags("ShiftHandoverNotes")
            .RequireAuthorization("ApiOrCookie");

        notes.MapGet("/", ListNotesAsync).WithName("ListShiftHandoverNotes");
        notes.MapGet("/{id:int}", GetNoteAsync).WithName("GetShiftHandoverNote");
        notes.MapPost("/", CreateNoteAsync).WithName("CreateShiftHandoverNote");
        notes.MapPost("/{id:int}/acknowledge", AcknowledgeNoteAsync).WithName("AcknowledgeShiftHandoverNote");
        notes.MapDelete("/{id:int}", DeleteNoteAsync).WithName("DeleteShiftHandoverNote")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));

        var ann = app.MapGroup("/api/announcements")
            .WithTags("Announcements")
            .RequireAuthorization("ApiOrCookie");

        ann.MapGet("/", ListAnnAsync).WithName("ListAnnouncements");
        ann.MapGet("/{id:int}", GetAnnAsync).WithName("GetAnnouncement");
        ann.MapPost("/", CreateAnnAsync).WithName("CreateAnnouncement")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));
        ann.MapPatch("/{id:int}", UpdateAnnAsync).WithName("UpdateAnnouncement")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));
        ann.MapDelete("/{id:int}", DeleteAnnAsync).WithName("DeleteAnnouncement")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Admin));
        ann.MapGet("/{id:int}/history", AnnHistoryAsync).WithName("ListAnnouncementHistory");

        return app;
    }

    private static async Task<Ok<PagedResult<ShiftHandoverNoteDto>>> ListNotesAsync(
        ApplicationDbContext db,
        [FromQuery] int skip = 0, [FromQuery] int take = 50,
        [FromQuery] int? stationId = null,
        [FromQuery] DateOnly? shiftDate = null,
        [FromQuery] bool? unacknowledgedOnly = null,
        CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 500);
        var q = db.ShiftHandoverNotes.AsNoTracking();
        if (stationId.HasValue) q = q.Where(x => x.StationId == stationId.Value);
        if (shiftDate.HasValue) q = q.Where(x => x.ShiftDate == shiftDate.Value);
        if (unacknowledgedOnly == true)
            q = q.Where(x => x.RequiresAcknowledgment && x.AcknowledgedAt == null);
        var total = await q.CountAsync(ct);
        var rows = await q.OrderByDescending(x => x.ShiftDate).ThenByDescending(x => x.Id)
            .Skip(skip).Take(take).Select(x => x.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<ShiftHandoverNoteDto>(rows, total, skip, take));
    }

    private static async Task<Results<Ok<ShiftHandoverNoteDto>, NotFound>> GetNoteAsync(
        int id, ApplicationDbContext db, CancellationToken ct)
    {
        var row = await db.ShiftHandoverNotes.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        return row is null ? TypedResults.NotFound() : TypedResults.Ok(row.ToDto());
    }

    private static async Task<Results<Created<IdResponse>, ValidationProblem>> CreateNoteAsync(
        CreateShiftHandoverNoteRequest request,
        IValidator<CreateShiftHandoverNoteRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());
        var entity = request.ToEntity(user.Identity?.Name);
        db.ShiftHandoverNotes.Add(entity);
        await db.SaveChangesAsync(ct);
        return TypedResults.Created($"/api/shift-handover-notes/{entity.Id}", new IdResponse(entity.Id));
    }

    private static async Task<Results<Ok<ShiftHandoverNoteDto>, NotFound>> AcknowledgeNoteAsync(
        int id, ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var entity = await db.ShiftHandoverNotes.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null) return TypedResults.NotFound();
        entity.AcknowledgedAt = DateTime.UtcNow;
        entity.AcknowledgedByUserId = user.Identity?.Name;
        entity.ModifiedDate = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return TypedResults.Ok(entity.ToDto());
    }

    private static async Task<Results<NoContent, NotFound>> DeleteNoteAsync(
        int id, ApplicationDbContext db, CancellationToken ct)
    {
        var entity = await db.ShiftHandoverNotes.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null) return TypedResults.NotFound();
        db.ShiftHandoverNotes.Remove(entity);
        await db.SaveChangesAsync(ct);
        return TypedResults.NoContent();
    }

    private static async Task<Ok<PagedResult<AnnouncementDto>>> ListAnnAsync(
        ApplicationDbContext db,
        [FromQuery] int skip = 0, [FromQuery] int take = 50,
        [FromQuery] int? organizationId = null,
        [FromQuery] int? orgUnitId = null,
        [FromQuery] AnnouncementSeverity? severity = null,
        [FromQuery] bool? activeOnly = null,
        CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 500);
        var q = db.Announcements.AsNoTracking();
        if (organizationId.HasValue) q = q.Where(x => x.OrganizationId == organizationId.Value);
        if (orgUnitId.HasValue) q = q.Where(x => x.OrgUnitId == orgUnitId.Value);
        if (severity.HasValue) q = q.Where(x => x.Severity == severity.Value);
        if (activeOnly == true)
        {
            var now = DateTime.UtcNow;
            q = q.Where(x => x.IsActive && (x.ExpiresAt == null || x.ExpiresAt > now));
        }
        var total = await q.CountAsync(ct);
        var rows = await q.OrderByDescending(x => x.PublishedAt).ThenByDescending(x => x.Id)
            .Skip(skip).Take(take).Select(x => x.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<AnnouncementDto>(rows, total, skip, take));
    }

    private static async Task<Results<Ok<AnnouncementDto>, NotFound>> GetAnnAsync(
        int id, ApplicationDbContext db, CancellationToken ct)
    {
        var row = await db.Announcements.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        return row is null ? TypedResults.NotFound() : TypedResults.Ok(row.ToDto());
    }

    private static async Task<Results<Created<IdResponse>, ValidationProblem>> CreateAnnAsync(
        CreateAnnouncementRequest request,
        IValidator<CreateAnnouncementRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());
        var entity = request.ToEntity(user.Identity?.Name);
        await db.AddWithAuditAsync(entity, e => AnnouncementAuditService.RecordCreate(e, user.Identity?.Name), ct);
        return TypedResults.Created($"/api/announcements/{entity.Id}", new IdResponse(entity.Id));
    }

    private static async Task<Results<Ok<IdResponse>, NotFound, ValidationProblem>> UpdateAnnAsync(
        int id, UpdateAnnouncementRequest request,
        IValidator<UpdateAnnouncementRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());
        var entity = await db.Announcements.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null) return TypedResults.NotFound();
        var before = AnnouncementAuditService.CaptureSnapshot(entity);
        request.ApplyTo(entity);
        db.AnnouncementAuditLogs.Add(AnnouncementAuditService.RecordUpdate(before, entity, user.Identity?.Name));
        await db.SaveChangesAsync(ct);
        return TypedResults.Ok(new IdResponse(entity.Id));
    }

    private static async Task<Results<NoContent, NotFound>> DeleteAnnAsync(
        int id, ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var entity = await db.Announcements.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null) return TypedResults.NotFound();
        await db.DeleteWithAuditAsync(entity, AnnouncementAuditService.RecordDelete(entity, user.Identity?.Name), ct);
        return TypedResults.NoContent();
    }

    private static async Task<Ok<PagedResult<AnnouncementAuditLogDto>>> AnnHistoryAsync(
        int id, ApplicationDbContext db,
        [FromQuery] int skip = 0, [FromQuery] int take = 50, CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 500);
        var q = db.AnnouncementAuditLogs.AsNoTracking().Where(a => a.AnnouncementId == id);
        var total = await q.CountAsync(ct);
        var rows = await q.OrderByDescending(a => a.ChangedDate).ThenByDescending(a => a.Id)
            .Skip(skip).Take(take).Select(a => a.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<AnnouncementAuditLogDto>(rows, total, skip, take));
    }
}
