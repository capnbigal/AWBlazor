using System.Security.Claims;
using AWBlazorApp.Data;
using AWBlazorApp.Infrastructure.Persistence;
using AWBlazorApp.Shared.Models;
using AWBlazorApp.Features.Mes.Audit;
using AWBlazorApp.Features.Mes.Domain;
using AWBlazorApp.Features.Mes.Models;
using AWBlazorApp.Features.Mes.Services;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.Mes.Endpoints;

public static class WorkInstructionEndpoints
{
    public static IEndpointRouteBuilder MapWorkInstructionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/work-instructions")
            .WithTags("WorkInstructions")
            .RequireAuthorization("ApiOrCookie");

        group.MapGet("/", ListAsync).WithName("ListWorkInstructions");
        group.MapGet("/{id:int}", GetAsync).WithName("GetWorkInstruction");
        group.MapPost("/", CreateAsync).WithName("CreateWorkInstruction")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));
        group.MapPatch("/{id:int}", UpdateAsync).WithName("UpdateWorkInstruction")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));
        group.MapDelete("/{id:int}", DeleteAsync).WithName("DeleteWorkInstruction")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));
        group.MapGet("/{id:int}/history", HistoryAsync).WithName("ListWorkInstructionHistory");

        group.MapGet("/{id:int}/revisions", ListRevisionsAsync).WithName("ListWorkInstructionRevisions");
        group.MapPost("/{id:int}/revisions", CreateRevisionAsync).WithName("CreateWorkInstructionRevision")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));
        group.MapPost("/revisions/{revId:int}/publish", PublishRevisionAsync).WithName("PublishWorkInstructionRevision")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));

        group.MapGet("/revisions/{revId:int}/steps", ListStepsAsync).WithName("ListWorkInstructionSteps");
        group.MapPost("/revisions/{revId:int}/steps", CreateStepAsync).WithName("CreateWorkInstructionStep")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));
        group.MapPatch("/steps/{stepId:int}", UpdateStepAsync).WithName("UpdateWorkInstructionStep")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));
        group.MapDelete("/steps/{stepId:int}", DeleteStepAsync).WithName("DeleteWorkInstructionStep")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));

        return app;
    }

    private static async Task<Ok<PagedResult<WorkInstructionDto>>> ListAsync(
        ApplicationDbContext db,
        [FromQuery] int skip = 0, [FromQuery] int take = 50,
        [FromQuery] int? workOrderRoutingId = null, [FromQuery] bool? isActive = null,
        CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 500);
        var q = db.WorkInstructions.AsNoTracking();
        if (workOrderRoutingId.HasValue) q = q.Where(w => w.WorkOrderRoutingId == workOrderRoutingId.Value);
        if (isActive.HasValue) q = q.Where(w => w.IsActive == isActive.Value);
        var total = await q.CountAsync(ct);
        var rows = await q.OrderBy(w => w.Title).Skip(skip).Take(take).Select(w => w.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<WorkInstructionDto>(rows, total, skip, take));
    }

    private static async Task<Results<Ok<WorkInstructionDto>, NotFound>> GetAsync(int id, ApplicationDbContext db, CancellationToken ct)
    {
        var row = await db.WorkInstructions.AsNoTracking().FirstOrDefaultAsync(w => w.Id == id, ct);
        return row is null ? TypedResults.NotFound() : TypedResults.Ok(row.ToDto());
    }

    private static async Task<Results<Created<IdResponse>, ValidationProblem>> CreateAsync(
        CreateWorkInstructionRequest request,
        IValidator<CreateWorkInstructionRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());
        var entity = request.ToEntity();
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        db.WorkInstructions.Add(entity);
        await db.SaveChangesAsync(ct);
        db.WorkInstructionAuditLogs.Add(WorkInstructionAuditService.RecordCreate(entity, user.Identity?.Name));
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return TypedResults.Created($"/api/work-instructions/{entity.Id}", new IdResponse(entity.Id));
    }

    private static async Task<Results<Ok<IdResponse>, NotFound, ValidationProblem>> UpdateAsync(
        int id, UpdateWorkInstructionRequest request,
        IValidator<UpdateWorkInstructionRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());
        var entity = await db.WorkInstructions.FirstOrDefaultAsync(w => w.Id == id, ct);
        if (entity is null) return TypedResults.NotFound();
        var before = WorkInstructionAuditService.CaptureSnapshot(entity);
        request.ApplyTo(entity);
        db.WorkInstructionAuditLogs.Add(WorkInstructionAuditService.RecordUpdate(before, entity, user.Identity?.Name));
        await db.SaveChangesAsync(ct);
        return TypedResults.Ok(new IdResponse(entity.Id));
    }

    private static async Task<Results<NoContent, NotFound>> DeleteAsync(
        int id, ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var entity = await db.WorkInstructions.FirstOrDefaultAsync(w => w.Id == id, ct);
        if (entity is null) return TypedResults.NotFound();
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        db.WorkInstructions.Remove(entity);
        db.WorkInstructionAuditLogs.Add(WorkInstructionAuditService.RecordDelete(entity, user.Identity?.Name));
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return TypedResults.NoContent();
    }

    private static async Task<Ok<PagedResult<WorkInstructionAuditLogDto>>> HistoryAsync(
        int id, ApplicationDbContext db,
        [FromQuery] int skip = 0, [FromQuery] int take = 50, CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 500);
        var q = db.WorkInstructionAuditLogs.AsNoTracking().Where(a => a.WorkInstructionId == id);
        var total = await q.CountAsync(ct);
        var rows = await q.OrderByDescending(a => a.ChangedDate).ThenByDescending(a => a.Id)
            .Skip(skip).Take(take).Select(a => a.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<WorkInstructionAuditLogDto>(rows, total, skip, take));
    }

    private static async Task<Ok<PagedResult<WorkInstructionRevisionDto>>> ListRevisionsAsync(
        int id, ApplicationDbContext db, CancellationToken ct = default)
    {
        var rows = await db.WorkInstructionRevisions.AsNoTracking()
            .Where(r => r.WorkInstructionId == id)
            .OrderByDescending(r => r.RevisionNumber)
            .Select(r => r.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<WorkInstructionRevisionDto>(rows, rows.Count, 0, rows.Count));
    }

    private static async Task<Results<Created<IdResponse>, NotFound, BadRequest<string>>> CreateRevisionAsync(
        int id, IWorkInstructionRevisionService svc, ClaimsPrincipal user, CancellationToken ct)
    {
        try
        {
            var revisionId = await svc.CreateNewRevisionAsync(id, user.Identity?.Name, ct);
            return TypedResults.Created($"/api/work-instructions/{id}/revisions/{revisionId}", new IdResponse(revisionId));
        }
        catch (InvalidOperationException ex) { return TypedResults.BadRequest(ex.Message); }
    }

    private static async Task<Results<Ok<IdResponse>, BadRequest<string>>> PublishRevisionAsync(
        int revId, IWorkInstructionRevisionService svc, ClaimsPrincipal user, CancellationToken ct)
    {
        try
        {
            await svc.PublishAsync(revId, user.Identity?.Name, ct);
            return TypedResults.Ok(new IdResponse(revId));
        }
        catch (InvalidOperationException ex) { return TypedResults.BadRequest(ex.Message); }
    }

    private static async Task<Ok<PagedResult<WorkInstructionStepDto>>> ListStepsAsync(
        int revId, ApplicationDbContext db, CancellationToken ct = default)
    {
        var rows = await db.WorkInstructionSteps.AsNoTracking()
            .Where(s => s.WorkInstructionRevisionId == revId)
            .OrderBy(s => s.SequenceNumber)
            .Select(s => s.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<WorkInstructionStepDto>(rows, rows.Count, 0, rows.Count));
    }

    private static async Task<Results<Created<IdResponse>, NotFound, ValidationProblem, BadRequest<string>>> CreateStepAsync(
        int revId, CreateWorkInstructionStepRequest request,
        IValidator<CreateWorkInstructionStepRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());
        var revision = await db.WorkInstructionRevisions.AsNoTracking().FirstOrDefaultAsync(r => r.Id == revId, ct);
        if (revision is null) return TypedResults.NotFound();
        if (revision.Status != WorkInstructionRevisionStatus.Draft)
            return TypedResults.BadRequest($"Revision is {revision.Status}; only Draft revisions can be edited.");

        request = request with { WorkInstructionRevisionId = revId };
        var entity = request.ToEntity();
        db.WorkInstructionSteps.Add(entity);
        await db.SaveChangesAsync(ct);
        db.WorkInstructionStepAuditLogs.Add(WorkInstructionStepAuditService.RecordCreate(entity, user.Identity?.Name));
        await db.SaveChangesAsync(ct);
        return TypedResults.Created($"/api/work-instructions/revisions/{revId}/steps/{entity.Id}", new IdResponse(entity.Id));
    }

    private static async Task<Results<Ok<IdResponse>, NotFound, ValidationProblem, BadRequest<string>>> UpdateStepAsync(
        int stepId, UpdateWorkInstructionStepRequest request,
        IValidator<UpdateWorkInstructionStepRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());
        var entity = await db.WorkInstructionSteps.FirstOrDefaultAsync(s => s.Id == stepId, ct);
        if (entity is null) return TypedResults.NotFound();
        var revision = await db.WorkInstructionRevisions.AsNoTracking().FirstAsync(r => r.Id == entity.WorkInstructionRevisionId, ct);
        if (revision.Status != WorkInstructionRevisionStatus.Draft)
            return TypedResults.BadRequest($"Revision is {revision.Status}; only Draft revisions can be edited.");

        var before = WorkInstructionStepAuditService.CaptureSnapshot(entity);
        request.ApplyTo(entity);
        db.WorkInstructionStepAuditLogs.Add(WorkInstructionStepAuditService.RecordUpdate(before, entity, user.Identity?.Name));
        await db.SaveChangesAsync(ct);
        return TypedResults.Ok(new IdResponse(entity.Id));
    }

    private static async Task<Results<NoContent, NotFound, BadRequest<string>>> DeleteStepAsync(
        int stepId, ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var entity = await db.WorkInstructionSteps.FirstOrDefaultAsync(s => s.Id == stepId, ct);
        if (entity is null) return TypedResults.NotFound();
        var revision = await db.WorkInstructionRevisions.AsNoTracking().FirstAsync(r => r.Id == entity.WorkInstructionRevisionId, ct);
        if (revision.Status != WorkInstructionRevisionStatus.Draft)
            return TypedResults.BadRequest($"Revision is {revision.Status}; only Draft revisions can be edited.");

        db.WorkInstructionSteps.Remove(entity);
        db.WorkInstructionStepAuditLogs.Add(WorkInstructionStepAuditService.RecordDelete(entity, user.Identity?.Name));
        await db.SaveChangesAsync(ct);
        return TypedResults.NoContent();
    }
}
