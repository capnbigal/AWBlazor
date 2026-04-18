using System.Security.Claims;
using AWBlazorApp.Data;
using AWBlazorApp.Features.Workforce.Audit;
using AWBlazorApp.Features.Workforce.Models;
using AWBlazorApp.Infrastructure.Persistence;
using AWBlazorApp.Shared.Models;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.Workforce.Endpoints;

public static class TrainingEndpoints
{
    public static IEndpointRouteBuilder MapTrainingEndpoints(this IEndpointRouteBuilder app)
    {
        var courses = app.MapGroup("/api/training-courses")
            .WithTags("TrainingCourses")
            .RequireAuthorization("ApiOrCookie");

        courses.MapGet("/", ListCoursesAsync).WithName("ListTrainingCourses");
        courses.MapGet("/{id:int}", GetCourseAsync).WithName("GetTrainingCourse");
        courses.MapPost("/", CreateCourseAsync).WithName("CreateTrainingCourse")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));
        courses.MapPatch("/{id:int}", UpdateCourseAsync).WithName("UpdateTrainingCourse")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));
        courses.MapDelete("/{id:int}", DeleteCourseAsync).WithName("DeleteTrainingCourse")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Admin));
        courses.MapGet("/{id:int}/history", CourseHistoryAsync).WithName("ListTrainingCourseHistory");

        var records = app.MapGroup("/api/training-records")
            .WithTags("TrainingRecords")
            .RequireAuthorization("ApiOrCookie");

        records.MapGet("/", ListRecordsAsync).WithName("ListTrainingRecords");
        records.MapGet("/{id:int}", GetRecordAsync).WithName("GetTrainingRecord");
        records.MapPost("/", CreateRecordAsync).WithName("CreateTrainingRecord")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));
        records.MapDelete("/{id:int}", DeleteRecordAsync).WithName("DeleteTrainingRecord")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Admin));

        return app;
    }

    private static async Task<Ok<PagedResult<TrainingCourseDto>>> ListCoursesAsync(
        ApplicationDbContext db,
        [FromQuery] int skip = 0, [FromQuery] int take = 50,
        [FromQuery] string? search = null,
        [FromQuery] bool? isActive = null,
        CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 500);
        var q = db.TrainingCourses.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            q = q.Where(x => x.Code.Contains(s) || x.Name.Contains(s));
        }
        if (isActive.HasValue) q = q.Where(x => x.IsActive == isActive.Value);
        var total = await q.CountAsync(ct);
        var rows = await q.OrderBy(x => x.Code)
            .Skip(skip).Take(take).Select(x => x.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<TrainingCourseDto>(rows, total, skip, take));
    }

    private static async Task<Results<Ok<TrainingCourseDto>, NotFound>> GetCourseAsync(
        int id, ApplicationDbContext db, CancellationToken ct)
    {
        var row = await db.TrainingCourses.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        return row is null ? TypedResults.NotFound() : TypedResults.Ok(row.ToDto());
    }

    private static async Task<Results<Created<IdResponse>, ValidationProblem>> CreateCourseAsync(
        CreateTrainingCourseRequest request,
        IValidator<CreateTrainingCourseRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());
        var entity = request.ToEntity();
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        db.TrainingCourses.Add(entity);
        await db.SaveChangesAsync(ct);
        db.TrainingCourseAuditLogs.Add(TrainingCourseAuditService.RecordCreate(entity, user.Identity?.Name));
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return TypedResults.Created($"/api/training-courses/{entity.Id}", new IdResponse(entity.Id));
    }

    private static async Task<Results<Ok<IdResponse>, NotFound, ValidationProblem>> UpdateCourseAsync(
        int id, UpdateTrainingCourseRequest request,
        IValidator<UpdateTrainingCourseRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());
        var entity = await db.TrainingCourses.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null) return TypedResults.NotFound();
        var before = TrainingCourseAuditService.CaptureSnapshot(entity);
        request.ApplyTo(entity);
        db.TrainingCourseAuditLogs.Add(TrainingCourseAuditService.RecordUpdate(before, entity, user.Identity?.Name));
        await db.SaveChangesAsync(ct);
        return TypedResults.Ok(new IdResponse(entity.Id));
    }

    private static async Task<Results<NoContent, NotFound>> DeleteCourseAsync(
        int id, ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var entity = await db.TrainingCourses.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null) return TypedResults.NotFound();
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        db.TrainingCourses.Remove(entity);
        db.TrainingCourseAuditLogs.Add(TrainingCourseAuditService.RecordDelete(entity, user.Identity?.Name));
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return TypedResults.NoContent();
    }

    private static async Task<Ok<PagedResult<TrainingCourseAuditLogDto>>> CourseHistoryAsync(
        int id, ApplicationDbContext db,
        [FromQuery] int skip = 0, [FromQuery] int take = 50, CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 500);
        var q = db.TrainingCourseAuditLogs.AsNoTracking().Where(a => a.TrainingCourseId == id);
        var total = await q.CountAsync(ct);
        var rows = await q.OrderByDescending(a => a.ChangedDate).ThenByDescending(a => a.Id)
            .Skip(skip).Take(take).Select(a => a.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<TrainingCourseAuditLogDto>(rows, total, skip, take));
    }

    private static async Task<Ok<PagedResult<TrainingRecordDto>>> ListRecordsAsync(
        ApplicationDbContext db,
        [FromQuery] int skip = 0, [FromQuery] int take = 50,
        [FromQuery] int? trainingCourseId = null,
        [FromQuery] int? businessEntityId = null,
        CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 500);
        var q = db.TrainingRecords.AsNoTracking();
        if (trainingCourseId.HasValue) q = q.Where(r => r.TrainingCourseId == trainingCourseId.Value);
        if (businessEntityId.HasValue) q = q.Where(r => r.BusinessEntityId == businessEntityId.Value);
        var total = await q.CountAsync(ct);
        var rows = await q.OrderByDescending(r => r.CompletedAt).ThenByDescending(r => r.Id)
            .Skip(skip).Take(take).Select(r => r.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<TrainingRecordDto>(rows, total, skip, take));
    }

    private static async Task<Results<Ok<TrainingRecordDto>, NotFound>> GetRecordAsync(
        int id, ApplicationDbContext db, CancellationToken ct)
    {
        var row = await db.TrainingRecords.AsNoTracking().FirstOrDefaultAsync(r => r.Id == id, ct);
        return row is null ? TypedResults.NotFound() : TypedResults.Ok(row.ToDto());
    }

    private static async Task<Results<Created<IdResponse>, NotFound, ValidationProblem>> CreateRecordAsync(
        CreateTrainingRecordRequest request,
        IValidator<CreateTrainingRecordRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());

        var course = await db.TrainingCourses.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == request.TrainingCourseId, ct);
        if (course is null) return TypedResults.NotFound();

        var now = request.CompletedAt ?? DateTime.UtcNow;
        var expires = course.RecurrenceMonths.HasValue
            ? now.AddMonths(course.RecurrenceMonths.Value)
            : (DateTime?)null;

        var entity = new AWBlazorApp.Features.Workforce.Domain.TrainingRecord
        {
            TrainingCourseId = request.TrainingCourseId,
            BusinessEntityId = request.BusinessEntityId,
            CompletedAt = now,
            ExpiresOn = expires,
            Score = request.Score?.Trim(),
            EvidenceUrl = request.EvidenceUrl?.Trim(),
            Notes = request.Notes?.Trim(),
            RecordedByUserId = user.Identity?.Name,
            ModifiedDate = DateTime.UtcNow,
        };
        db.TrainingRecords.Add(entity);
        await db.SaveChangesAsync(ct);
        return TypedResults.Created($"/api/training-records/{entity.Id}", new IdResponse(entity.Id));
    }

    private static async Task<Results<NoContent, NotFound>> DeleteRecordAsync(
        int id, ApplicationDbContext db, CancellationToken ct)
    {
        var entity = await db.TrainingRecords.FirstOrDefaultAsync(r => r.Id == id, ct);
        if (entity is null) return TypedResults.NotFound();
        db.TrainingRecords.Remove(entity);
        await db.SaveChangesAsync(ct);
        return TypedResults.NoContent();
    }
}
