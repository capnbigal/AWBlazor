using System.Security.Claims;
using AWBlazorApp.Data;
using AWBlazorApp.Infrastructure.Persistence;
using AWBlazorApp.Shared.Models;
using AWBlazorApp.Features.Sales.Models;
using AWBlazorApp.Features.Sales.Audit;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.Sales.Endpoints;

public static class SalesOrderDetailEndpoints
{
    public static IEndpointRouteBuilder MapSalesOrderDetailEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/aw/sales-order-details")
            .WithTags("SalesOrderDetails")
            .RequireAuthorization("ApiOrCookie");

        group.MapGet("/", ListAsync).WithName("ListSalesOrderDetails")
            .WithSummary("List Sales.SalesOrderDetail rows. Composite PK = (SalesOrderID, SalesOrderDetailID).");
        group.MapGet("/by-key", GetAsync).WithName("GetSalesOrderDetail");
        group.MapPost("/", CreateAsync).WithName("CreateSalesOrderDetail")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));
        group.MapPatch("/by-key", UpdateAsync).WithName("UpdateSalesOrderDetail")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));
        group.MapDelete("/by-key", DeleteAsync).WithName("DeleteSalesOrderDetail")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));
        group.MapGet("/history", HistoryAsync).WithName("ListSalesOrderDetailHistory");
        return app;
    }

    private static async Task<Ok<PagedResult<SalesOrderDetailDto>>> ListAsync(
        ApplicationDbContext db, [FromQuery] int skip = 0, [FromQuery] int take = 50,
        [FromQuery] int? salesOrderId = null, [FromQuery] int? productId = null,
        CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 1000);
        var query = db.SalesOrderDetails.AsNoTracking();
        if (salesOrderId.HasValue) query = query.Where(x => x.SalesOrderId == salesOrderId.Value);
        if (productId.HasValue) query = query.Where(x => x.ProductId == productId.Value);
        var total = await query.CountAsync(ct);
        var rows = await query.OrderBy(x => x.SalesOrderId).ThenBy(x => x.SalesOrderDetailId)
            .Skip(skip).Take(take).Select(x => x.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<SalesOrderDetailDto>(rows, total, skip, take));
    }

    private static async Task<Results<Ok<SalesOrderDetailDto>, NotFound>> GetAsync(
        [FromQuery] int salesOrderId, [FromQuery] int salesOrderDetailId,
        ApplicationDbContext db, CancellationToken ct)
    {
        var row = await db.SalesOrderDetails.AsNoTracking()
            .FirstOrDefaultAsync(x => x.SalesOrderId == salesOrderId && x.SalesOrderDetailId == salesOrderDetailId, ct);
        return row is null ? TypedResults.NotFound() : TypedResults.Ok(row.ToDto());
    }

    private static async Task<Results<Created<CompositeKeyResponse>, ValidationProblem>> CreateAsync(
        CreateSalesOrderDetailRequest request, IValidator<CreateSalesOrderDetailRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());

        var entity = request.ToEntity();
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        db.SalesOrderDetails.Add(entity);
        await db.SaveChangesAsync(ct);
        // Re-query to pick up computed LineTotal and the identity SalesOrderDetailId.
        var reloaded = await db.SalesOrderDetails.AsNoTracking()
            .FirstAsync(x => x.SalesOrderId == entity.SalesOrderId && x.SalesOrderDetailId == entity.SalesOrderDetailId, ct);
        db.SalesOrderDetailAuditLogs.Add(SalesOrderDetailAuditService.RecordCreate(reloaded, user.Identity?.Name));
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return TypedResults.Created(
            $"/api/aw/sales-order-details/by-key?salesOrderId={entity.SalesOrderId}&salesOrderDetailId={entity.SalesOrderDetailId}",
            new CompositeKeyResponse(new Dictionary<string, object>
            {
                ["salesOrderId"] = entity.SalesOrderId,
                ["salesOrderDetailId"] = entity.SalesOrderDetailId,
            }));
    }

    private static async Task<Results<Ok<CompositeKeyResponse>, NotFound, ValidationProblem>> UpdateAsync(
        [FromQuery] int salesOrderId, [FromQuery] int salesOrderDetailId,
        UpdateSalesOrderDetailRequest request, IValidator<UpdateSalesOrderDetailRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());

        var entity = await db.SalesOrderDetails
            .FirstOrDefaultAsync(x => x.SalesOrderId == salesOrderId && x.SalesOrderDetailId == salesOrderDetailId, ct);
        if (entity is null) return TypedResults.NotFound();

        var before = SalesOrderDetailAuditService.CaptureSnapshot(entity);
        request.ApplyTo(entity);
        db.SalesOrderDetailAuditLogs.Add(
            SalesOrderDetailAuditService.RecordUpdate(before, entity, user.Identity?.Name));
        await db.SaveChangesAsync(ct);
        return TypedResults.Ok(new CompositeKeyResponse(new Dictionary<string, object>
        {
            ["salesOrderId"] = entity.SalesOrderId,
            ["salesOrderDetailId"] = entity.SalesOrderDetailId,
        }));
    }

    private static async Task<Results<NoContent, NotFound>> DeleteAsync(
        [FromQuery] int salesOrderId, [FromQuery] int salesOrderDetailId,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var entity = await db.SalesOrderDetails
            .FirstOrDefaultAsync(x => x.SalesOrderId == salesOrderId && x.SalesOrderDetailId == salesOrderDetailId, ct);
        if (entity is null) return TypedResults.NotFound();

        db.SalesOrderDetailAuditLogs.Add(SalesOrderDetailAuditService.RecordDelete(entity, user.Identity?.Name));
        db.SalesOrderDetails.Remove(entity);
        await db.SaveChangesAsync(ct);
        return TypedResults.NoContent();
    }

    private static async Task<Ok<List<SalesOrderDetailAuditLogDto>>> HistoryAsync(
        ApplicationDbContext db,
        [FromQuery] int? salesOrderId = null,
        [FromQuery] int? salesOrderDetailId = null,
        CancellationToken ct = default)
    {
        var query = db.SalesOrderDetailAuditLogs.AsNoTracking();
        if (salesOrderId.HasValue) query = query.Where(a => a.SalesOrderId == salesOrderId.Value);
        if (salesOrderDetailId.HasValue) query = query.Where(a => a.SalesOrderDetailId == salesOrderDetailId.Value);

        var rows = await query
            .OrderByDescending(a => a.ChangedDate).ThenByDescending(a => a.Id)
            .Select(a => a.ToDto())
            .ToListAsync(ct);
        return TypedResults.Ok(rows);
    }
}
