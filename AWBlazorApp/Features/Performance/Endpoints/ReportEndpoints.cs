using System.Security.Claims;
using AWBlazorApp.Data;
using AWBlazorApp.Features.Performance.Audit;
using AWBlazorApp.Features.Performance.Models;
using AWBlazorApp.Infrastructure.Persistence;
using AWBlazorApp.Shared.Models;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.Performance.Endpoints;

public static class ReportEndpoints
{
    public static IEndpointRouteBuilder MapReportEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/performance-reports")
            .WithTags("PerformanceReports")
            .RequireAuthorization("ApiOrCookie");

        group.MapGet("/", ListAsync).WithName("ListPerformanceReports");
        group.MapGet("/{id:int}", GetAsync).WithName("GetPerformanceReport");
        group.MapPost("/", CreateAsync).WithName("CreatePerformanceReport")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));
        group.MapPatch("/{id:int}", UpdateAsync).WithName("UpdatePerformanceReport")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));
        group.MapDelete("/{id:int}", DeleteAsync).WithName("DeletePerformanceReport")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Admin));
        group.MapGet("/{id:int}/history", HistoryAsync).WithName("ListPerformanceReportHistory");
        group.MapGet("/{id:int}/runs", ListRunsAsync).WithName("ListPerformanceReportRuns");

        return app;
    }

    private static async Task<Ok<PagedResult<PerformanceReportDto>>> ListAsync(
        ApplicationDbContext db,
        [FromQuery] int skip = 0, [FromQuery] int take = 50,
        [FromQuery] bool? isActive = null,
        CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 500);
        var q = db.PerformanceReports.AsNoTracking();
        if (isActive.HasValue) q = q.Where(x => x.IsActive == isActive.Value);
        var total = await q.CountAsync(ct);
        var rows = await q.OrderBy(x => x.Code)
            .Skip(skip).Take(take).Select(x => x.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<PerformanceReportDto>(rows, total, skip, take));
    }

    private static async Task<Results<Ok<PerformanceReportDto>, NotFound>> GetAsync(int id, ApplicationDbContext db, CancellationToken ct)
    {
        var row = await db.PerformanceReports.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        return row is null ? TypedResults.NotFound() : TypedResults.Ok(row.ToDto());
    }

    private static async Task<Results<Created<IdResponse>, ValidationProblem>> CreateAsync(
        CreatePerformanceReportRequest request,
        IValidator<CreatePerformanceReportRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());
        var entity = request.ToEntity();
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        db.PerformanceReports.Add(entity);
        await db.SaveChangesAsync(ct);
        db.PerformanceReportAuditLogs.Add(PerformanceReportAuditService.RecordCreate(entity, user.Identity?.Name));
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return TypedResults.Created($"/api/performance-reports/{entity.Id}", new IdResponse(entity.Id));
    }

    private static async Task<Results<Ok<IdResponse>, NotFound, ValidationProblem>> UpdateAsync(
        int id, UpdatePerformanceReportRequest request,
        IValidator<UpdatePerformanceReportRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());
        var entity = await db.PerformanceReports.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null) return TypedResults.NotFound();
        var before = PerformanceReportAuditService.CaptureSnapshot(entity);
        request.ApplyTo(entity);
        db.PerformanceReportAuditLogs.Add(PerformanceReportAuditService.RecordUpdate(before, entity, user.Identity?.Name));
        await db.SaveChangesAsync(ct);
        return TypedResults.Ok(new IdResponse(entity.Id));
    }

    private static async Task<Results<NoContent, NotFound>> DeleteAsync(
        int id, ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var entity = await db.PerformanceReports.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null) return TypedResults.NotFound();
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        db.PerformanceReports.Remove(entity);
        db.PerformanceReportAuditLogs.Add(PerformanceReportAuditService.RecordDelete(entity, user.Identity?.Name));
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return TypedResults.NoContent();
    }

    private static async Task<Ok<PagedResult<PerformanceReportAuditLogDto>>> HistoryAsync(
        int id, ApplicationDbContext db,
        [FromQuery] int skip = 0, [FromQuery] int take = 50, CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 500);
        var q = db.PerformanceReportAuditLogs.AsNoTracking().Where(a => a.PerformanceReportId == id);
        var total = await q.CountAsync(ct);
        var rows = await q.OrderByDescending(a => a.ChangedDate).ThenByDescending(a => a.Id)
            .Skip(skip).Take(take).Select(a => a.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<PerformanceReportAuditLogDto>(rows, total, skip, take));
    }

    private static async Task<Ok<PagedResult<PerformanceReportRunDto>>> ListRunsAsync(
        int id, ApplicationDbContext db,
        [FromQuery] int skip = 0, [FromQuery] int take = 50, CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 500);
        var q = db.PerformanceReportRuns.AsNoTracking().Where(r => r.PerformanceReportId == id);
        var total = await q.CountAsync(ct);
        var rows = await q.OrderByDescending(r => r.RunAt)
            .Skip(skip).Take(take).Select(r => r.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<PerformanceReportRunDto>(rows, total, skip, take));
    }
}
