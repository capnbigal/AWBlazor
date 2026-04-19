using AWBlazorApp.Features.Mes.Runs.Dtos;
using System.Security.Claims;
using AWBlazorApp.Features.Identity.Domain; using AWBlazorApp.Features.Admin.Permissions.Domain;
using AWBlazorApp.Infrastructure.Persistence;
using AWBlazorApp.Shared.Dtos;
using AWBlazorApp.Features.Mes.Audit;
using AWBlazorApp.Features.Mes.Downtime.Domain; using AWBlazorApp.Features.Mes.Instructions.Domain; using AWBlazorApp.Features.Mes.Runs.Domain; 
using AWBlazorApp.Features.Mes.Dtos;
using AWBlazorApp.Features.Mes.Runs.Application.Services; using AWBlazorApp.Features.Mes.Instructions.Application.Services; 
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.Mes.Runs.Api;

public static class ProductionRunEndpoints
{
    public static IEndpointRouteBuilder MapProductionRunEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/production-runs")
            .WithTags("ProductionRuns")
            .RequireAuthorization("ApiOrCookie");

        group.MapGet("/", ListAsync).WithName("ListProductionRuns");
        group.MapGet("/{id:int}", GetAsync).WithName("GetProductionRun");
        group.MapPost("/", CreateAsync).WithName("CreateProductionRun")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));
        group.MapPatch("/{id:int}", UpdateAsync).WithName("UpdateProductionRun")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));
        group.MapDelete("/{id:int}", DeleteAsync).WithName("DeleteProductionRun")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));
        group.MapGet("/{id:int}/history", HistoryAsync).WithName("ListProductionRunHistory");

        group.MapPost("/{id:int}/start", StartAsync).WithName("StartProductionRun")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));
        group.MapPost("/{id:int}/pause", PauseAsync).WithName("PauseProductionRun")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));
        group.MapPost("/{id:int}/resume", ResumeAsync).WithName("ResumeProductionRun")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));
        group.MapPost("/{id:int}/cancel", CancelAsync).WithName("CancelProductionRun")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));
        group.MapPost("/{id:int}/complete", CompleteAsync).WithName("CompleteProductionRun")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));

        group.MapGet("/{id:int}/operations", ListOperationsAsync).WithName("ListProductionRunOperations");
        group.MapPost("/{id:int}/operations", AddOperationAsync).WithName("CreateProductionRunOperation")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));
        group.MapPatch("/operations/{opId:int}", UpdateOperationAsync).WithName("UpdateProductionRunOperation")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));
        group.MapDelete("/operations/{opId:int}", DeleteOperationAsync).WithName("DeleteProductionRunOperation")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));

        return app;
    }

    private static async Task<Ok<PagedResult<ProductionRunDto>>> ListAsync(
        ApplicationDbContext db,
        [FromQuery] int skip = 0, [FromQuery] int take = 50,
        [FromQuery] ProductionRunStatus? status = null,
        [FromQuery] ProductionRunKind? kind = null,
        [FromQuery] int? stationId = null,
        [FromQuery] int? workOrderId = null,
        CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 500);
        var q = db.ProductionRuns.AsNoTracking();
        if (status.HasValue) q = q.Where(x => x.Status == status.Value);
        if (kind.HasValue) q = q.Where(x => x.Kind == kind.Value);
        if (stationId.HasValue) q = q.Where(x => x.StationId == stationId.Value);
        if (workOrderId.HasValue) q = q.Where(x => x.WorkOrderId == workOrderId.Value);
        var total = await q.CountAsync(ct);
        var rows = await q.OrderByDescending(x => x.PlannedStartAt ?? x.ModifiedDate).ThenByDescending(x => x.Id)
            .Skip(skip).Take(take).Select(x => x.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<ProductionRunDto>(rows, total, skip, take));
    }

    private static async Task<Results<Ok<ProductionRunDto>, NotFound>> GetAsync(int id, ApplicationDbContext db, CancellationToken ct)
    {
        var row = await db.ProductionRuns.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        return row is null ? TypedResults.NotFound() : TypedResults.Ok(row.ToDto());
    }

    private static async Task<Results<Created<IdResponse>, ValidationProblem>> CreateAsync(
        CreateProductionRunRequest request,
        IValidator<CreateProductionRunRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());
        var entity = request.ToEntity();
        await db.AddWithAuditAsync(entity, e => ProductionRunAuditService.RecordCreate(e, user.Identity?.Name), ct);
        return TypedResults.Created($"/api/production-runs/{entity.Id}", new IdResponse(entity.Id));
    }

    private static async Task<Results<Ok<IdResponse>, NotFound, ValidationProblem>> UpdateAsync(
        int id, UpdateProductionRunRequest request,
        IValidator<UpdateProductionRunRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());
        var entity = await db.ProductionRuns.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null) return TypedResults.NotFound();
        if (entity.Status is ProductionRunStatus.Completed or ProductionRunStatus.Cancelled)
            return TypedResults.ValidationProblem(new Dictionary<string, string[]> { ["Status"] = [$"Run is {entity.Status}; no further edits allowed."] });

        var before = ProductionRunAuditService.CaptureSnapshot(entity);
        request.ApplyTo(entity);
        db.ProductionRunAuditLogs.Add(ProductionRunAuditService.RecordUpdate(before, entity, user.Identity?.Name));
        await db.SaveChangesAsync(ct);
        return TypedResults.Ok(new IdResponse(entity.Id));
    }

    private static async Task<Results<NoContent, NotFound, ValidationProblem>> DeleteAsync(
        int id, ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var entity = await db.ProductionRuns.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null) return TypedResults.NotFound();
        if (entity.Status == ProductionRunStatus.Completed)
            return TypedResults.ValidationProblem(new Dictionary<string, string[]> { ["Status"] = ["Cannot delete a completed run; cancel it instead."] });

        await db.DeleteWithAuditAsync(entity, ProductionRunAuditService.RecordDelete(entity, user.Identity?.Name), ct);
        return TypedResults.NoContent();
    }

    private static async Task<Results<Ok<RunTransitionResult>, BadRequest<string>>> StartAsync(int id, IProductionRunService svc, ClaimsPrincipal user, CancellationToken ct)
    { try { return TypedResults.Ok(await svc.StartAsync(id, user.Identity?.Name, ct)); } catch (InvalidOperationException ex) { return TypedResults.BadRequest(ex.Message); } }

    private static async Task<Results<Ok<RunTransitionResult>, BadRequest<string>>> PauseAsync(int id, IProductionRunService svc, ClaimsPrincipal user, CancellationToken ct)
    { try { return TypedResults.Ok(await svc.PauseAsync(id, user.Identity?.Name, ct)); } catch (InvalidOperationException ex) { return TypedResults.BadRequest(ex.Message); } }

    private static async Task<Results<Ok<RunTransitionResult>, BadRequest<string>>> ResumeAsync(int id, IProductionRunService svc, ClaimsPrincipal user, CancellationToken ct)
    { try { return TypedResults.Ok(await svc.ResumeAsync(id, user.Identity?.Name, ct)); } catch (InvalidOperationException ex) { return TypedResults.BadRequest(ex.Message); } }

    private static async Task<Results<Ok<RunTransitionResult>, BadRequest<string>>> CancelAsync(int id, IProductionRunService svc, ClaimsPrincipal user, CancellationToken ct)
    { try { return TypedResults.Ok(await svc.CancelAsync(id, user.Identity?.Name, ct)); } catch (InvalidOperationException ex) { return TypedResults.BadRequest(ex.Message); } }

    private static async Task<Results<Ok<RunCompletionResult>, BadRequest<string>, ValidationProblem>> CompleteAsync(
        int id, CompleteProductionRunRequest request,
        IValidator<CompleteProductionRunRequest> validator,
        IProductionRunService svc, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());

        MaterialIssueRequest? issue = null;
        if (request.MaterialIssueQuantity is { } q && request.MaterialIssueInventoryItemId is { } itemId
            && request.MaterialIssueFromLocationId is { } fromLoc
            && !string.IsNullOrWhiteSpace(request.MaterialIssueUnitMeasureCode))
        {
            issue = new MaterialIssueRequest(itemId, q, request.MaterialIssueUnitMeasureCode, fromLoc, request.MaterialIssueLotId);
        }

        try
        {
            var result = await svc.CompleteAsync(id, request.QuantityProduced, request.QuantityScrapped, issue, user.Identity?.Name, ct);
            return TypedResults.Ok(result);
        }
        catch (InvalidOperationException ex) { return TypedResults.BadRequest(ex.Message); }
    }

    private static async Task<Ok<PagedResult<ProductionRunAuditLogDto>>> HistoryAsync(
        int id, ApplicationDbContext db,
        [FromQuery] int skip = 0, [FromQuery] int take = 50, CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 500);
        var q = db.ProductionRunAuditLogs.AsNoTracking().Where(a => a.ProductionRunId == id);
        var total = await q.CountAsync(ct);
        var rows = await q.OrderByDescending(a => a.ChangedDate).ThenByDescending(a => a.Id)
            .Skip(skip).Take(take).Select(a => a.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<ProductionRunAuditLogDto>(rows, total, skip, take));
    }

    private static async Task<Ok<PagedResult<ProductionRunOperationDto>>> ListOperationsAsync(
        int id, ApplicationDbContext db,
        [FromQuery] int skip = 0, [FromQuery] int take = 500, CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 1000);
        var q = db.ProductionRunOperations.AsNoTracking().Where(o => o.ProductionRunId == id);
        var total = await q.CountAsync(ct);
        var rows = await q.OrderBy(o => o.SequenceNumber).Skip(skip).Take(take).Select(o => o.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<ProductionRunOperationDto>(rows, total, skip, take));
    }

    private static async Task<Results<Created<IdResponse>, NotFound, ValidationProblem>> AddOperationAsync(
        int id, CreateProductionRunOperationRequest request,
        IValidator<CreateProductionRunOperationRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());
        var run = await db.ProductionRuns.FirstOrDefaultAsync(r => r.Id == id, ct);
        if (run is null) return TypedResults.NotFound();
        request = request with { ProductionRunId = id };
        var entity = request.ToEntity();
        db.ProductionRunOperations.Add(entity);
        await db.SaveChangesAsync(ct);
        db.ProductionRunOperationAuditLogs.Add(ProductionRunOperationAuditService.RecordCreate(entity, user.Identity?.Name));
        await db.SaveChangesAsync(ct);
        return TypedResults.Created($"/api/production-runs/{id}/operations/{entity.Id}", new IdResponse(entity.Id));
    }

    private static async Task<Results<Ok<IdResponse>, NotFound, ValidationProblem>> UpdateOperationAsync(
        int opId, UpdateProductionRunOperationRequest request,
        IValidator<UpdateProductionRunOperationRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());
        var entity = await db.ProductionRunOperations.FirstOrDefaultAsync(x => x.Id == opId, ct);
        if (entity is null) return TypedResults.NotFound();
        var before = ProductionRunOperationAuditService.CaptureSnapshot(entity);
        request.ApplyTo(entity);
        db.ProductionRunOperationAuditLogs.Add(ProductionRunOperationAuditService.RecordUpdate(before, entity, user.Identity?.Name));
        await db.SaveChangesAsync(ct);
        return TypedResults.Ok(new IdResponse(entity.Id));
    }

    private static async Task<Results<NoContent, NotFound>> DeleteOperationAsync(
        int opId, ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var entity = await db.ProductionRunOperations.FirstOrDefaultAsync(x => x.Id == opId, ct);
        if (entity is null) return TypedResults.NotFound();
        db.ProductionRunOperations.Remove(entity);
        db.ProductionRunOperationAuditLogs.Add(ProductionRunOperationAuditService.RecordDelete(entity, user.Identity?.Name));
        await db.SaveChangesAsync(ct);
        return TypedResults.NoContent();
    }
}
