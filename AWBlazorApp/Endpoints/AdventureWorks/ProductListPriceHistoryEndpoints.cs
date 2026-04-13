using System.Security.Claims;
using AWBlazorApp.Data;
using AWBlazorApp.Models;
using AWBlazorApp.Models.AdventureWorks;
using AWBlazorApp.Services.AdventureWorksAudit;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Endpoints.AdventureWorks;

public static class ProductListPriceHistoryEndpoints
{
    public static IEndpointRouteBuilder MapProductListPriceHistoryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/aw/product-list-price-histories")
            .WithTags("ProductListPriceHistories")
            .RequireAuthorization("ApiOrCookie");

        group.MapGet("/", ListAsync).WithName("ListProductListPriceHistories")
            .WithSummary("List Production.ProductListPriceHistory rows. Composite PK = (ProductID, StartDate).");
        group.MapGet("/by-key", GetAsync).WithName("GetProductListPriceHistory");
        group.MapPost("/", CreateAsync).WithName("CreateProductListPriceHistory")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));
        group.MapPatch("/by-key", UpdateAsync).WithName("UpdateProductListPriceHistory")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));
        group.MapDelete("/by-key", DeleteAsync).WithName("DeleteProductListPriceHistory")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));
        group.MapGet("/history", HistoryAsync).WithName("ListProductListPriceHistoryHistory");
        return app;
    }

    private static async Task<Ok<PagedResult<ProductListPriceHistoryDto>>> ListAsync(
        ApplicationDbContext db, [FromQuery] int skip = 0, [FromQuery] int take = 50,
        [FromQuery] int? productId = null, CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 1000);
        var query = db.ProductListPriceHistories.AsNoTracking();
        if (productId.HasValue) query = query.Where(x => x.ProductId == productId.Value);
        var total = await query.CountAsync(ct);
        var rows = await query.OrderBy(x => x.ProductId).ThenByDescending(x => x.StartDate)
            .Skip(skip).Take(take).Select(x => x.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<ProductListPriceHistoryDto>(rows, total, skip, take));
    }

    private static async Task<Results<Ok<ProductListPriceHistoryDto>, NotFound>> GetAsync(
        [FromQuery] int productId, [FromQuery] DateTime startDate,
        ApplicationDbContext db, CancellationToken ct)
    {
        var row = await db.ProductListPriceHistories.AsNoTracking()
            .FirstOrDefaultAsync(x => x.ProductId == productId && x.StartDate == startDate, ct);
        return row is null ? TypedResults.NotFound() : TypedResults.Ok(row.ToDto());
    }

    private static async Task<Results<Created<CompositeKeyResponse>, Conflict<string>, ValidationProblem>> CreateAsync(
        CreateProductListPriceHistoryRequest request, IValidator<CreateProductListPriceHistoryRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());

        if (await db.ProductListPriceHistories.AnyAsync(x =>
                x.ProductId == request.ProductId && x.StartDate == request.StartDate, ct))
        {
            return TypedResults.Conflict($"List price row ({request.ProductId}, {request.StartDate:O}) already exists.");
        }

        var entity = request.ToEntity();
        db.ProductListPriceHistories.Add(entity);
        await db.SaveChangesAsync(ct);
        db.ProductListPriceHistoryAuditLogs.Add(
            ProductListPriceHistoryAuditService.RecordCreate(entity, user.Identity?.Name));
        await db.SaveChangesAsync(ct);
        return TypedResults.Created(
            $"/api/aw/product-list-price-histories/by-key?productId={entity.ProductId}&startDate={entity.StartDate:O}",
            new CompositeKeyResponse(new Dictionary<string, object>
            {
                ["productId"] = entity.ProductId,
                ["startDate"] = entity.StartDate,
            }));
    }

    private static async Task<Results<Ok<CompositeKeyResponse>, NotFound, ValidationProblem>> UpdateAsync(
        [FromQuery] int productId, [FromQuery] DateTime startDate,
        UpdateProductListPriceHistoryRequest request, IValidator<UpdateProductListPriceHistoryRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());

        var entity = await db.ProductListPriceHistories
            .FirstOrDefaultAsync(x => x.ProductId == productId && x.StartDate == startDate, ct);
        if (entity is null) return TypedResults.NotFound();

        var before = ProductListPriceHistoryAuditService.CaptureSnapshot(entity);
        request.ApplyTo(entity);
        db.ProductListPriceHistoryAuditLogs.Add(
            ProductListPriceHistoryAuditService.RecordUpdate(before, entity, user.Identity?.Name));
        await db.SaveChangesAsync(ct);
        return TypedResults.Ok(new CompositeKeyResponse(new Dictionary<string, object>
        {
            ["productId"] = entity.ProductId,
            ["startDate"] = entity.StartDate,
        }));
    }

    private static async Task<Results<NoContent, NotFound>> DeleteAsync(
        [FromQuery] int productId, [FromQuery] DateTime startDate,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var entity = await db.ProductListPriceHistories
            .FirstOrDefaultAsync(x => x.ProductId == productId && x.StartDate == startDate, ct);
        if (entity is null) return TypedResults.NotFound();

        db.ProductListPriceHistoryAuditLogs.Add(
            ProductListPriceHistoryAuditService.RecordDelete(entity, user.Identity?.Name));
        db.ProductListPriceHistories.Remove(entity);
        await db.SaveChangesAsync(ct);
        return TypedResults.NoContent();
    }

    private static async Task<Ok<List<ProductListPriceHistoryAuditLogDto>>> HistoryAsync(
        ApplicationDbContext db,
        [FromQuery] int? productId = null,
        [FromQuery] DateTime? startDate = null,
        CancellationToken ct = default)
    {
        var query = db.ProductListPriceHistoryAuditLogs.AsNoTracking();
        if (productId.HasValue) query = query.Where(a => a.ProductId == productId.Value);
        if (startDate.HasValue) query = query.Where(a => a.StartDate == startDate.Value);

        var rows = await query
            .OrderByDescending(a => a.ChangedDate).ThenByDescending(a => a.Id)
            .Select(a => a.ToDto())
            .ToListAsync(ct);
        return TypedResults.Ok(rows);
    }
}
