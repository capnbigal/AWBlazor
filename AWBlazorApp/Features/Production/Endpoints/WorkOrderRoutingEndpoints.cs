using System.Security.Claims;
using AWBlazorApp.Data;
using AWBlazorApp.Infrastructure.Persistence;
using AWBlazorApp.Shared.Models;
using AWBlazorApp.Features.Production.Models;
using AWBlazorApp.Features.Production.Audit;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.Production.Endpoints;

public static class WorkOrderRoutingEndpoints
{
    public static IEndpointRouteBuilder MapWorkOrderRoutingEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/aw/work-order-routings")
            .WithTags("WorkOrderRoutings")
            .RequireAuthorization("ApiOrCookie");

        group.MapGet("/", ListAsync).WithName("ListWorkOrderRoutings")
            .WithSummary("List Production.WorkOrderRouting rows. 3-col composite PK = (WorkOrderID, ProductID, OperationSequence).");
        group.MapGet("/by-key", GetAsync).WithName("GetWorkOrderRouting");
        group.MapPost("/", CreateAsync).WithName("CreateWorkOrderRouting")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));
        group.MapPatch("/by-key", UpdateAsync).WithName("UpdateWorkOrderRouting")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));
        group.MapDelete("/by-key", DeleteAsync).WithName("DeleteWorkOrderRouting")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));
        group.MapGet("/history", HistoryAsync).WithName("ListWorkOrderRoutingHistory");
        return app;
    }

    private static async Task<Ok<PagedResult<WorkOrderRoutingDto>>> ListAsync(
        ApplicationDbContext db, [FromQuery] int skip = 0, [FromQuery] int take = 50,
        [FromQuery] int? workOrderId = null, CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 1000);
        var query = db.WorkOrderRoutings.AsNoTracking();
        if (workOrderId.HasValue) query = query.Where(x => x.WorkOrderId == workOrderId.Value);
        var total = await query.CountAsync(ct);
        var rows = await query.OrderBy(x => x.WorkOrderId).ThenBy(x => x.ProductId).ThenBy(x => x.OperationSequence)
            .Skip(skip).Take(take).Select(x => x.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<WorkOrderRoutingDto>(rows, total, skip, take));
    }

    private static async Task<Results<Ok<WorkOrderRoutingDto>, NotFound>> GetAsync(
        [FromQuery] int workOrderId, [FromQuery] int productId, [FromQuery] short operationSequence,
        ApplicationDbContext db, CancellationToken ct)
    {
        var row = await db.WorkOrderRoutings.AsNoTracking()
            .FirstOrDefaultAsync(x => x.WorkOrderId == workOrderId
                && x.ProductId == productId
                && x.OperationSequence == operationSequence, ct);
        return row is null ? TypedResults.NotFound() : TypedResults.Ok(row.ToDto());
    }

    private static async Task<Results<Created<CompositeKeyResponse>, Conflict<string>, ValidationProblem>> CreateAsync(
        CreateWorkOrderRoutingRequest request, IValidator<CreateWorkOrderRoutingRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());

        if (await db.WorkOrderRoutings.AnyAsync(x =>
                x.WorkOrderId == request.WorkOrderId
                && x.ProductId == request.ProductId
                && x.OperationSequence == request.OperationSequence, ct))
        {
            return TypedResults.Conflict($"Row ({request.WorkOrderId}, {request.ProductId}, {request.OperationSequence}) already exists.");
        }

        var entity = request.ToEntity();
        await db.AddWithAuditAsync(entity, e => WorkOrderRoutingAuditService.RecordCreate(e, user.Identity?.Name), ct);
        return TypedResults.Created(
            $"/api/aw/work-order-routings/by-key?workOrderId={entity.WorkOrderId}&productId={entity.ProductId}&operationSequence={entity.OperationSequence}",
            new CompositeKeyResponse(new Dictionary<string, object>
            {
                ["workOrderId"] = entity.WorkOrderId,
                ["productId"] = entity.ProductId,
                ["operationSequence"] = entity.OperationSequence,
            }));
    }

    private static async Task<Results<Ok<CompositeKeyResponse>, NotFound, ValidationProblem>> UpdateAsync(
        [FromQuery] int workOrderId, [FromQuery] int productId, [FromQuery] short operationSequence,
        UpdateWorkOrderRoutingRequest request, IValidator<UpdateWorkOrderRoutingRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());

        var entity = await db.WorkOrderRoutings
            .FirstOrDefaultAsync(x => x.WorkOrderId == workOrderId
                && x.ProductId == productId
                && x.OperationSequence == operationSequence, ct);
        if (entity is null) return TypedResults.NotFound();

        var before = WorkOrderRoutingAuditService.CaptureSnapshot(entity);
        request.ApplyTo(entity);
        db.WorkOrderRoutingAuditLogs.Add(
            WorkOrderRoutingAuditService.RecordUpdate(before, entity, user.Identity?.Name));
        await db.SaveChangesAsync(ct);
        return TypedResults.Ok(new CompositeKeyResponse(new Dictionary<string, object>
        {
            ["workOrderId"] = entity.WorkOrderId,
            ["productId"] = entity.ProductId,
            ["operationSequence"] = entity.OperationSequence,
        }));
    }

    private static async Task<Results<NoContent, NotFound>> DeleteAsync(
        [FromQuery] int workOrderId, [FromQuery] int productId, [FromQuery] short operationSequence,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var entity = await db.WorkOrderRoutings
            .FirstOrDefaultAsync(x => x.WorkOrderId == workOrderId
                && x.ProductId == productId
                && x.OperationSequence == operationSequence, ct);
        if (entity is null) return TypedResults.NotFound();

        db.WorkOrderRoutingAuditLogs.Add(
            WorkOrderRoutingAuditService.RecordDelete(entity, user.Identity?.Name));
        db.WorkOrderRoutings.Remove(entity);
        await db.SaveChangesAsync(ct);
        return TypedResults.NoContent();
    }

    private static async Task<Ok<List<WorkOrderRoutingAuditLogDto>>> HistoryAsync(
        ApplicationDbContext db,
        [FromQuery] int? workOrderId = null,
        [FromQuery] int? productId = null,
        [FromQuery] short? operationSequence = null,
        CancellationToken ct = default)
    {
        var query = db.WorkOrderRoutingAuditLogs.AsNoTracking();
        if (workOrderId.HasValue) query = query.Where(a => a.WorkOrderId == workOrderId.Value);
        if (productId.HasValue) query = query.Where(a => a.ProductId == productId.Value);
        if (operationSequence.HasValue) query = query.Where(a => a.OperationSequence == operationSequence.Value);

        var rows = await query
            .OrderByDescending(a => a.ChangedDate).ThenByDescending(a => a.Id)
            .Select(a => a.ToDto())
            .ToListAsync(ct);
        return TypedResults.Ok(rows);
    }
}
