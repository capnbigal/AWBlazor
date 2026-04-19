using System.Security.Claims;
using AWBlazorApp.Features.Identity.Domain; using AWBlazorApp.Features.Admin.Permissions.Domain;
using AWBlazorApp.Infrastructure.Persistence;
using AWBlazorApp.Shared.Dtos;
using AWBlazorApp.Features.Sales.Dtos;
using AWBlazorApp.Features.Sales.Audit;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.Sales.Api;

public static class SalesOrderHeaderSalesReasonEndpoints
{
    public static IEndpointRouteBuilder MapSalesOrderHeaderSalesReasonEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/aw/sales-order-header-sales-reasons")
            .WithTags("SalesOrderHeaderSalesReasons")
            .RequireAuthorization("ApiOrCookie");

        group.MapGet("/", ListAsync).WithName("ListSalesOrderHeaderSalesReasons")
            .WithSummary("List Sales.SalesOrderHeaderSalesReason rows. Composite PK = (SalesOrderID, SalesReasonID).");
        group.MapGet("/by-key", GetAsync).WithName("GetSalesOrderHeaderSalesReason");
        group.MapPost("/", CreateAsync).WithName("CreateSalesOrderHeaderSalesReason")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));
        group.MapPatch("/by-key", UpdateAsync).WithName("UpdateSalesOrderHeaderSalesReason")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));
        group.MapDelete("/by-key", DeleteAsync).WithName("DeleteSalesOrderHeaderSalesReason")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));
        group.MapGet("/history", HistoryAsync).WithName("ListSalesOrderHeaderSalesReasonHistory");
        return app;
    }

    private static async Task<Ok<PagedResult<SalesOrderHeaderSalesReasonDto>>> ListAsync(
        ApplicationDbContext db, [FromQuery] int skip = 0, [FromQuery] int take = 50,
        [FromQuery] int? salesOrderId = null, CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 1000);
        var query = db.SalesOrderHeaderSalesReasons.AsNoTracking();
        if (salesOrderId.HasValue) query = query.Where(x => x.SalesOrderId == salesOrderId.Value);
        var total = await query.CountAsync(ct);
        var rows = await query.OrderBy(x => x.SalesOrderId).ThenBy(x => x.SalesReasonId)
            .Skip(skip).Take(take).Select(x => x.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<SalesOrderHeaderSalesReasonDto>(rows, total, skip, take));
    }

    private static async Task<Results<Ok<SalesOrderHeaderSalesReasonDto>, NotFound>> GetAsync(
        [FromQuery] int salesOrderId, [FromQuery] int salesReasonId,
        ApplicationDbContext db, CancellationToken ct)
    {
        var row = await db.SalesOrderHeaderSalesReasons.AsNoTracking()
            .FirstOrDefaultAsync(x => x.SalesOrderId == salesOrderId && x.SalesReasonId == salesReasonId, ct);
        return row is null ? TypedResults.NotFound() : TypedResults.Ok(row.ToDto());
    }

    private static async Task<Results<Created<CompositeKeyResponse>, Conflict<string>, ValidationProblem>> CreateAsync(
        CreateSalesOrderHeaderSalesReasonRequest request,
        IValidator<CreateSalesOrderHeaderSalesReasonRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());

        if (await db.SalesOrderHeaderSalesReasons.AnyAsync(x =>
                x.SalesOrderId == request.SalesOrderId && x.SalesReasonId == request.SalesReasonId, ct))
        {
            return TypedResults.Conflict($"Junction row ({request.SalesOrderId}, {request.SalesReasonId}) already exists.");
        }

        var entity = request.ToEntity();
        await db.AddWithAuditAsync(entity, e => SalesOrderHeaderSalesReasonAuditService.RecordCreate(e, user.Identity?.Name), ct);
        return TypedResults.Created(
            $"/api/aw/sales-order-header-sales-reasons/by-key?salesOrderId={entity.SalesOrderId}&salesReasonId={entity.SalesReasonId}",
            new CompositeKeyResponse(new Dictionary<string, object>
            {
                ["salesOrderId"] = entity.SalesOrderId,
                ["salesReasonId"] = entity.SalesReasonId,
            }));
    }

    private static async Task<Results<Ok<CompositeKeyResponse>, NotFound, ValidationProblem>> UpdateAsync(
        [FromQuery] int salesOrderId, [FromQuery] int salesReasonId,
        UpdateSalesOrderHeaderSalesReasonRequest request,
        IValidator<UpdateSalesOrderHeaderSalesReasonRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());

        var entity = await db.SalesOrderHeaderSalesReasons
            .FirstOrDefaultAsync(x => x.SalesOrderId == salesOrderId && x.SalesReasonId == salesReasonId, ct);
        if (entity is null) return TypedResults.NotFound();

        request.ApplyTo(entity);
        db.SalesOrderHeaderSalesReasonAuditLogs.Add(
            SalesOrderHeaderSalesReasonAuditService.RecordUpdate(entity, user.Identity?.Name));
        await db.SaveChangesAsync(ct);
        return TypedResults.Ok(new CompositeKeyResponse(new Dictionary<string, object>
        {
            ["salesOrderId"] = entity.SalesOrderId,
            ["salesReasonId"] = entity.SalesReasonId,
        }));
    }

    private static async Task<Results<NoContent, NotFound>> DeleteAsync(
        [FromQuery] int salesOrderId, [FromQuery] int salesReasonId,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var entity = await db.SalesOrderHeaderSalesReasons
            .FirstOrDefaultAsync(x => x.SalesOrderId == salesOrderId && x.SalesReasonId == salesReasonId, ct);
        if (entity is null) return TypedResults.NotFound();

        db.SalesOrderHeaderSalesReasonAuditLogs.Add(
            SalesOrderHeaderSalesReasonAuditService.RecordDelete(entity, user.Identity?.Name));
        db.SalesOrderHeaderSalesReasons.Remove(entity);
        await db.SaveChangesAsync(ct);
        return TypedResults.NoContent();
    }

    private static async Task<Ok<List<SalesOrderHeaderSalesReasonAuditLogDto>>> HistoryAsync(
        ApplicationDbContext db,
        [FromQuery] int? salesOrderId = null,
        [FromQuery] int? salesReasonId = null,
        CancellationToken ct = default)
    {
        var query = db.SalesOrderHeaderSalesReasonAuditLogs.AsNoTracking();
        if (salesOrderId.HasValue) query = query.Where(a => a.SalesOrderId == salesOrderId.Value);
        if (salesReasonId.HasValue) query = query.Where(a => a.SalesReasonId == salesReasonId.Value);

        var rows = await query
            .OrderByDescending(a => a.ChangedDate).ThenByDescending(a => a.Id)
            .Select(a => a.ToDto())
            .ToListAsync(ct);
        return TypedResults.Ok(rows);
    }
}
