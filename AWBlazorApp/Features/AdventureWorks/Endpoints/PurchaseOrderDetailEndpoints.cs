using System.Security.Claims;
using AWBlazorApp.Data;
using AWBlazorApp.Infrastructure.Persistence;
using AWBlazorApp.Models;
using AWBlazorApp.Features.AdventureWorks.Models;
using AWBlazorApp.Features.AdventureWorks.Audit;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.AdventureWorks.Endpoints;

public static class PurchaseOrderDetailEndpoints
{
    public static IEndpointRouteBuilder MapPurchaseOrderDetailEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/aw/purchase-order-details")
            .WithTags("PurchaseOrderDetails")
            .RequireAuthorization("ApiOrCookie");

        group.MapGet("/", ListAsync).WithName("ListPurchaseOrderDetails")
            .WithSummary("List Purchasing.PurchaseOrderDetail rows. Composite PK = (PurchaseOrderID, PurchaseOrderDetailID).");
        group.MapGet("/by-key", GetAsync).WithName("GetPurchaseOrderDetail");
        group.MapPost("/", CreateAsync).WithName("CreatePurchaseOrderDetail")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));
        group.MapPatch("/by-key", UpdateAsync).WithName("UpdatePurchaseOrderDetail")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));
        group.MapDelete("/by-key", DeleteAsync).WithName("DeletePurchaseOrderDetail")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));
        group.MapGet("/history", HistoryAsync).WithName("ListPurchaseOrderDetailHistory");
        return app;
    }

    private static async Task<Ok<PagedResult<PurchaseOrderDetailDto>>> ListAsync(
        ApplicationDbContext db, [FromQuery] int skip = 0, [FromQuery] int take = 50,
        [FromQuery] int? purchaseOrderId = null, CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 1000);
        var query = db.PurchaseOrderDetails.AsNoTracking();
        if (purchaseOrderId.HasValue) query = query.Where(x => x.PurchaseOrderId == purchaseOrderId.Value);
        var total = await query.CountAsync(ct);
        var rows = await query.OrderBy(x => x.PurchaseOrderId).ThenBy(x => x.PurchaseOrderDetailId)
            .Skip(skip).Take(take).Select(x => x.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<PurchaseOrderDetailDto>(rows, total, skip, take));
    }

    private static async Task<Results<Ok<PurchaseOrderDetailDto>, NotFound>> GetAsync(
        [FromQuery] int purchaseOrderId, [FromQuery] int purchaseOrderDetailId,
        ApplicationDbContext db, CancellationToken ct)
    {
        var row = await db.PurchaseOrderDetails.AsNoTracking()
            .FirstOrDefaultAsync(x => x.PurchaseOrderId == purchaseOrderId && x.PurchaseOrderDetailId == purchaseOrderDetailId, ct);
        return row is null ? TypedResults.NotFound() : TypedResults.Ok(row.ToDto());
    }

    private static async Task<Results<Created<CompositeKeyResponse>, ValidationProblem>> CreateAsync(
        CreatePurchaseOrderDetailRequest request, IValidator<CreatePurchaseOrderDetailRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());

        var entity = request.ToEntity();
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        db.PurchaseOrderDetails.Add(entity);
        await db.SaveChangesAsync(ct);
        db.PurchaseOrderDetailAuditLogs.Add(PurchaseOrderDetailAuditService.RecordCreate(entity, user.Identity?.Name));
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return TypedResults.Created(
            $"/api/aw/purchase-order-details/by-key?purchaseOrderId={entity.PurchaseOrderId}&purchaseOrderDetailId={entity.PurchaseOrderDetailId}",
            new CompositeKeyResponse(new Dictionary<string, object>
            {
                ["purchaseOrderId"] = entity.PurchaseOrderId,
                ["purchaseOrderDetailId"] = entity.PurchaseOrderDetailId,
            }));
    }

    private static async Task<Results<Ok<CompositeKeyResponse>, NotFound, ValidationProblem>> UpdateAsync(
        [FromQuery] int purchaseOrderId, [FromQuery] int purchaseOrderDetailId,
        UpdatePurchaseOrderDetailRequest request, IValidator<UpdatePurchaseOrderDetailRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());

        var entity = await db.PurchaseOrderDetails
            .FirstOrDefaultAsync(x => x.PurchaseOrderId == purchaseOrderId && x.PurchaseOrderDetailId == purchaseOrderDetailId, ct);
        if (entity is null) return TypedResults.NotFound();

        var before = PurchaseOrderDetailAuditService.CaptureSnapshot(entity);
        request.ApplyTo(entity);
        db.PurchaseOrderDetailAuditLogs.Add(
            PurchaseOrderDetailAuditService.RecordUpdate(before, entity, user.Identity?.Name));
        await db.SaveChangesAsync(ct);
        return TypedResults.Ok(new CompositeKeyResponse(new Dictionary<string, object>
        {
            ["purchaseOrderId"] = entity.PurchaseOrderId,
            ["purchaseOrderDetailId"] = entity.PurchaseOrderDetailId,
        }));
    }

    private static async Task<Results<NoContent, NotFound>> DeleteAsync(
        [FromQuery] int purchaseOrderId, [FromQuery] int purchaseOrderDetailId,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var entity = await db.PurchaseOrderDetails
            .FirstOrDefaultAsync(x => x.PurchaseOrderId == purchaseOrderId && x.PurchaseOrderDetailId == purchaseOrderDetailId, ct);
        if (entity is null) return TypedResults.NotFound();

        db.PurchaseOrderDetailAuditLogs.Add(PurchaseOrderDetailAuditService.RecordDelete(entity, user.Identity?.Name));
        db.PurchaseOrderDetails.Remove(entity);
        await db.SaveChangesAsync(ct);
        return TypedResults.NoContent();
    }

    private static async Task<Ok<List<PurchaseOrderDetailAuditLogDto>>> HistoryAsync(
        ApplicationDbContext db,
        [FromQuery] int? purchaseOrderId = null,
        [FromQuery] int? purchaseOrderDetailId = null,
        CancellationToken ct = default)
    {
        var query = db.PurchaseOrderDetailAuditLogs.AsNoTracking();
        if (purchaseOrderId.HasValue) query = query.Where(a => a.PurchaseOrderId == purchaseOrderId.Value);
        if (purchaseOrderDetailId.HasValue) query = query.Where(a => a.PurchaseOrderDetailId == purchaseOrderDetailId.Value);

        var rows = await query
            .OrderByDescending(a => a.ChangedDate).ThenByDescending(a => a.Id)
            .Select(a => a.ToDto())
            .ToListAsync(ct);
        return TypedResults.Ok(rows);
    }
}
