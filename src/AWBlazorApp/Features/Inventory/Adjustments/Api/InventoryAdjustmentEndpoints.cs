using System.Security.Claims;
using AWBlazorApp.Features.Identity.Domain; using AWBlazorApp.Features.Admin.Permissions.Domain;
using AWBlazorApp.Infrastructure.Persistence;
using AWBlazorApp.Shared.Audit;
using AWBlazorApp.Shared.Dtos;
using AWBlazorApp.Features.Inventory.Adjustments.Domain; using AWBlazorApp.Features.Inventory.Items.Domain; using AWBlazorApp.Features.Inventory.Locations.Domain; using AWBlazorApp.Features.Inventory.Lots.Domain; using AWBlazorApp.Features.Inventory.Outbox.Domain; using AWBlazorApp.Features.Inventory.Queue.Domain; using AWBlazorApp.Features.Inventory.Reports.Domain; using AWBlazorApp.Features.Inventory.Serials.Domain; using AWBlazorApp.Features.Inventory.Transactions.Domain; using AWBlazorApp.Features.Inventory.Types.Domain; 
using AWBlazorApp.Features.Inventory.Adjustments.Dtos; using AWBlazorApp.Features.Inventory.Items.Dtos; using AWBlazorApp.Features.Inventory.Locations.Dtos; using AWBlazorApp.Features.Inventory.Lots.Dtos; using AWBlazorApp.Features.Inventory.Serials.Dtos; 
using AWBlazorApp.Features.Inventory.Services;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.Inventory.Adjustments.Api;

/// <summary>
/// Adjustment CRUD plus the approve/reject lifecycle. Approve posts one <c>InventoryTransaction</c>
/// via <c>IInventoryService</c> (ADJUST_INC or ADJUST_DEC depending on sign of QuantityDelta) and
/// stores the resulting transaction id on the adjustment. Reject just flips status and audits.
/// </summary>
public static class InventoryAdjustmentEndpoints
{
    public static IEndpointRouteBuilder MapInventoryAdjustmentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/inventory-adjustments")
            .WithTags("InventoryAdjustments")
            .RequireAuthorization("ApiOrCookie");

        group.MapGet("/", ListAsync).WithName("ListInventoryAdjustments");
        group.MapGet("/{id:int}", GetAsync).WithName("GetInventoryAdjustment");
        group.MapPost("/", CreateAsync).WithName("CreateInventoryAdjustment")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));
        group.MapPatch("/{id:int}", UpdateAsync).WithName("UpdateInventoryAdjustment")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));
        group.MapPost("/{id:int}/approve", ApproveAsync).WithName("ApproveInventoryAdjustment")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));
        group.MapPost("/{id:int}/reject", RejectAsync).WithName("RejectInventoryAdjustment")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));
        group.MapGet("/{id:int}/history", HistoryAsync).WithName("ListInventoryAdjustmentHistory");

        return app;
    }

    private static async Task<Ok<PagedResult<InventoryAdjustmentDto>>> ListAsync(
        ApplicationDbContext db,
        [FromQuery] int skip = 0, [FromQuery] int take = 50,
        [FromQuery] AdjustmentStatus? status = null, [FromQuery] int? inventoryItemId = null,
        [FromQuery] int? locationId = null, [FromQuery] AdjustmentReason? reasonCode = null,
        CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 500);
        var query = db.InventoryAdjustments.AsNoTracking();
        if (status.HasValue) query = query.Where(x => x.Status == status.Value);
        if (inventoryItemId.HasValue) query = query.Where(x => x.InventoryItemId == inventoryItemId.Value);
        if (locationId.HasValue) query = query.Where(x => x.LocationId == locationId.Value);
        if (reasonCode.HasValue) query = query.Where(x => x.ReasonCode == reasonCode.Value);
        var total = await query.CountAsync(ct);
        var rows = await query.OrderByDescending(x => x.RequestedAt)
            .Skip(skip).Take(take).Select(x => x.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<InventoryAdjustmentDto>(rows, total, skip, take));
    }

    private static async Task<Results<Ok<InventoryAdjustmentDto>, NotFound>> GetAsync(
        int id, ApplicationDbContext db, CancellationToken ct)
    {
        var row = await db.InventoryAdjustments.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        return row is null ? TypedResults.NotFound() : TypedResults.Ok(row.ToDto());
    }

    private static async Task<Results<Created<IdResponse>, ValidationProblem>> CreateAsync(
        CreateInventoryAdjustmentRequest request,
        IValidator<CreateInventoryAdjustmentRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());

        var entity = request.ToEntity(user.Identity?.Name);
        db.InventoryAdjustments.Add(entity);
        await db.SaveChangesAsync(ct);
        return TypedResults.Created($"/api/inventory-adjustments/{entity.Id}", new IdResponse(entity.Id));
    }

    private static async Task<Results<Ok<IdResponse>, NotFound, ValidationProblem>> UpdateAsync(
        int id, UpdateInventoryAdjustmentRequest request,
        IValidator<UpdateInventoryAdjustmentRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());

        var entity = await db.InventoryAdjustments.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null) return TypedResults.NotFound();
        if (entity.Status is AdjustmentStatus.Posted or AdjustmentStatus.Approved or AdjustmentStatus.Rejected)
        {
            return TypedResults.ValidationProblem(new Dictionary<string, string[]>
            {
                ["Status"] = [$"Adjustment is {entity.Status}; no further edits allowed."],
            });
        }

        request.ApplyTo(entity);
        await db.SaveChangesAsync(ct);
        return TypedResults.Ok(new IdResponse(entity.Id));
    }

    private static async Task<Results<Ok<IdResponse>, NotFound, BadRequest<string>, ValidationProblem>> ApproveAsync(
        int id, ApplicationDbContext db, IInventoryService service, ClaimsPrincipal user, CancellationToken ct)
    {
        var entity = await db.InventoryAdjustments.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null) return TypedResults.NotFound();
        if (entity.Status is AdjustmentStatus.Posted or AdjustmentStatus.Rejected)
            return TypedResults.BadRequest($"Adjustment is already {entity.Status}.");

        var item = await db.InventoryItems.AsNoTracking().FirstOrDefaultAsync(i => i.Id == entity.InventoryItemId, ct);
        if (item is null) return TypedResults.BadRequest("InventoryItem no longer exists.");

        var typeCode = entity.QuantityDelta > 0
            ? InventoryTransactionTypeCodes.AdjustInc
            : InventoryTransactionTypeCodes.AdjustDec;

        try
        {
            var postRequest = new PostTransactionRequest(
                typeCode,
                entity.InventoryItemId,
                Math.Abs(entity.QuantityDelta),
                UnitMeasureCode: "EA",
                FromLocationId: entity.QuantityDelta > 0 ? null : entity.LocationId,
                ToLocationId: entity.QuantityDelta > 0 ? entity.LocationId : null,
                LotId: entity.LotId,
                SerialUnitId: null,
                FromStatus: null,
                ToStatus: null,
                ReferenceType: TransactionReferenceKind.Adjustment,
                ReferenceId: entity.Id,
                ReferenceLineId: null,
                Notes: entity.Reason,
                CorrelationId: null,
                OccurredAt: null);

            var result = await service.PostTransactionAsync(postRequest, user.Identity?.Name, ct);
            entity.Status = AdjustmentStatus.Posted;
            entity.ApprovedByUserId = user.Identity?.Name;
            entity.ApprovedAt = DateTime.UtcNow;
            entity.PostedTransactionId = result.TransactionId;
            entity.ModifiedDate = DateTime.UtcNow;
            await db.SaveChangesAsync(ct);
            return TypedResults.Ok(new IdResponse(entity.Id));
        }
        catch (InvalidOperationException ex)
        {
            return TypedResults.BadRequest(ex.Message);
        }
    }

    private static async Task<Results<Ok<IdResponse>, NotFound, BadRequest<string>>> RejectAsync(
        int id, ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var entity = await db.InventoryAdjustments.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null) return TypedResults.NotFound();
        if (entity.Status is AdjustmentStatus.Posted or AdjustmentStatus.Rejected)
            return TypedResults.BadRequest($"Adjustment is already {entity.Status}.");

        entity.Status = AdjustmentStatus.Rejected;
        entity.ApprovedByUserId = user.Identity?.Name;
        entity.ApprovedAt = DateTime.UtcNow;
        entity.ModifiedDate = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return TypedResults.Ok(new IdResponse(entity.Id));
    }

    private static async Task<Ok<PagedResult<AuditLog>>> HistoryAsync(
        int id, ApplicationDbContext db,
        [FromQuery] int skip = 0, [FromQuery] int take = 50, CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 500);
        var idStr = id.ToString();
        var query = db.AuditLogs.AsNoTracking().Where(a => a.EntityType == "InventoryAdjustment" && a.EntityId == idStr);
        var total = await query.CountAsync(ct);
        var rows = await query.OrderByDescending(x => x.ChangedDate).ThenByDescending(x => x.Id)
            .Skip(skip).Take(take).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<AuditLog>(rows, total, skip, take));
    }
}
