using AWBlazorApp.Data;
using AWBlazorApp.Infrastructure.Persistence;
using AWBlazorApp.Shared.Endpoints;
using AWBlazorApp.Shared.Models;
using AWBlazorApp.Features.HumanResources.Audit;
using AWBlazorApp.Features.HumanResources.Domain;
using AWBlazorApp.Features.HumanResources.Models;
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

        group.MapIntIdCrud<JobCandidate, JobCandidateDto, CreateJobCandidateRequest, UpdateJobCandidateRequest, JobCandidateAuditLog, JobCandidateAuditLogDto, JobCandidateAuditService.Snapshot, int>(
            entityName: "JobCandidate",
            routePrefix: "/api/aw/job-candidates",
            entitySet: db => db.JobCandidates,
            auditSet: db => db.JobCandidateAuditLogs,
            idSelector: e => e.Id,
            auditIdSelector: a => a.JobCandidateId,
            auditChangedDateSelector: a => a.ChangedDate,
            auditPrimaryKeySelector: a => a.Id,
            getId: e => e.Id,
            toDto: e => e.ToDto(),
            toEntity: r => r.ToEntity(),
            applyUpdate: (r, e) => r.ApplyTo(e),
            captureSnapshot: JobCandidateAuditService.CaptureSnapshot,
            recordCreate: JobCandidateAuditService.RecordCreate,
            recordUpdate: JobCandidateAuditService.RecordUpdate,
            recordDelete: JobCandidateAuditService.RecordDelete,
            auditToDto: a => a.ToDto());

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
}