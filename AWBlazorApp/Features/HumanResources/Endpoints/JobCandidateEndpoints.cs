using System.Security.Claims;
using AWBlazorApp.Data;
using AWBlazorApp.Infrastructure.Persistence;
using AWBlazorApp.Shared.Models;
using AWBlazorApp.Features.HumanResources.Models;
using AWBlazorApp.Features.HumanResources.Audit;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.HumanResources.Endpoints;

public static class JobCandidateEndpoints
{
    public static IEndpointRouteBuilder MapJobCandidateEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/aw/job-candidates")
            .WithTags("JobCandidates")
            .RequireAuthorization("ApiOrCookie");

        group.MapGet("/", ListAsync).WithName("ListJobCandidates").WithSummary("List HumanResources.JobCandidate rows.");
        group.MapGet("/{id:int}", GetAsync).WithName("GetJobCandidate");
        group.MapPost("/", CreateAsync).WithName("CreateJobCandidate")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));
        group.MapPatch("/{id:int}", UpdateAsync).WithName("UpdateJobCandidate")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));
        group.MapDelete("/{id:int}", DeleteAsync).WithName("DeleteJobCandidate")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));
        group.MapGet("/{id:int}/history", HistoryAsync).WithName("ListJobCandidateHistory");
        return app;
    }

    private static async Task<Ok<PagedResult<JobCandidateDto>>> ListAsync(
        ApplicationDbContext db, [FromQuery] int skip = 0, [FromQuery] int take = 50,
        [FromQuery] int? businessEntityId = null, CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 1000);
        var query = db.JobCandidates.AsNoTracking();
        if (businessEntityId.HasValue) query = query.Where(x => x.BusinessEntityId == businessEntityId.Value);
        var total = await query.CountAsync(ct);
        var rows = await query.OrderBy(x => x.Id).Skip(skip).Take(take).Select(x => x.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<JobCandidateDto>(rows, total, skip, take));
    }

    private static async Task<Results<Ok<JobCandidateDto>, NotFound>> GetAsync(int id, ApplicationDbContext db, CancellationToken ct)
    {
        var row = await db.JobCandidates.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        return row is null ? TypedResults.NotFound() : TypedResults.Ok(row.ToDto());
    }

    private static async Task<Results<Created<IdResponse>, ValidationProblem>> CreateAsync(
        CreateJobCandidateRequest request, IValidator<CreateJobCandidateRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());

        var entity = request.ToEntity();
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        db.JobCandidates.Add(entity);
        await db.SaveChangesAsync(ct);
        db.JobCandidateAuditLogs.Add(JobCandidateAuditService.RecordCreate(entity, user.Identity?.Name));
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return TypedResults.Created($"/api/aw/job-candidates/{entity.Id}", new IdResponse(entity.Id));
    }

    private static async Task<Results<Ok<IdResponse>, NotFound, ValidationProblem>> UpdateAsync(
        int id, UpdateJobCandidateRequest request, IValidator<UpdateJobCandidateRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());

        var entity = await db.JobCandidates.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null) return TypedResults.NotFound();

        var before = JobCandidateAuditService.CaptureSnapshot(entity);
        request.ApplyTo(entity);
        db.JobCandidateAuditLogs.Add(JobCandidateAuditService.RecordUpdate(before, entity, user.Identity?.Name));
        await db.SaveChangesAsync(ct);
        return TypedResults.Ok(new IdResponse(entity.Id));
    }

    private static async Task<Results<NoContent, NotFound>> DeleteAsync(
        int id, ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var entity = await db.JobCandidates.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null) return TypedResults.NotFound();

        db.JobCandidateAuditLogs.Add(JobCandidateAuditService.RecordDelete(entity, user.Identity?.Name));
        db.JobCandidates.Remove(entity);
        await db.SaveChangesAsync(ct);
        return TypedResults.NoContent();
    }

    private static async Task<Ok<List<JobCandidateAuditLogDto>>> HistoryAsync(int id, ApplicationDbContext db, CancellationToken ct)
    {
        var rows = await db.JobCandidateAuditLogs.AsNoTracking()
            .Where(a => a.JobCandidateId == id)
            .OrderByDescending(a => a.ChangedDate).ThenByDescending(a => a.Id)
            .Select(a => a.ToDto())
            .ToListAsync(ct);
        return TypedResults.Ok(rows);
    }
}
