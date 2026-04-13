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

public static class ProductInventoryEndpoints
{
    public static IEndpointRouteBuilder MapProductInventoryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/aw/product-inventories")
            .WithTags("ProductInventories")
            .RequireAuthorization("ApiOrCookie");

        group.MapGet("/", ListAsync).WithName("ListProductInventories")
            .WithSummary("List Production.ProductInventory rows. Composite PK = (ProductID, LocationID).");
        group.MapGet("/by-key", GetAsync).WithName("GetProductInventory");
        group.MapPost("/", CreateAsync).WithName("CreateProductInventory")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));
        group.MapPatch("/by-key", UpdateAsync).WithName("UpdateProductInventory")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));
        group.MapDelete("/by-key", DeleteAsync).WithName("DeleteProductInventory")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));
        group.MapGet("/history", HistoryAsync).WithName("ListProductInventoryHistory");
        return app;
    }

    private static async Task<Ok<PagedResult<ProductInventoryDto>>> ListAsync(
        ApplicationDbContext db, [FromQuery] int skip = 0, [FromQuery] int take = 50,
        [FromQuery] int? productId = null, [FromQuery] short? locationId = null,
        CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 1000);
        var query = db.ProductInventories.AsNoTracking();
        if (productId.HasValue) query = query.Where(x => x.ProductId == productId.Value);
        if (locationId.HasValue) query = query.Where(x => x.LocationId == locationId.Value);
        var total = await query.CountAsync(ct);
        var rows = await query.OrderBy(x => x.ProductId).ThenBy(x => x.LocationId)
            .Skip(skip).Take(take).Select(x => x.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<ProductInventoryDto>(rows, total, skip, take));
    }

    private static async Task<Results<Ok<ProductInventoryDto>, NotFound>> GetAsync(
        [FromQuery] int productId, [FromQuery] short locationId,
        ApplicationDbContext db, CancellationToken ct)
    {
        var row = await db.ProductInventories.AsNoTracking()
            .FirstOrDefaultAsync(x => x.ProductId == productId && x.LocationId == locationId, ct);
        return row is null ? TypedResults.NotFound() : TypedResults.Ok(row.ToDto());
    }

    private static async Task<Results<Created<CompositeKeyResponse>, Conflict<string>, ValidationProblem>> CreateAsync(
        CreateProductInventoryRequest request, IValidator<CreateProductInventoryRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());

        if (await db.ProductInventories.AnyAsync(x =>
                x.ProductId == request.ProductId && x.LocationId == request.LocationId, ct))
        {
            return TypedResults.Conflict($"Inventory row ({request.ProductId}, {request.LocationId}) already exists.");
        }

        var entity = request.ToEntity();
        db.ProductInventories.Add(entity);
        await db.SaveChangesAsync(ct);
        db.ProductInventoryAuditLogs.Add(ProductInventoryAuditService.RecordCreate(entity, user.Identity?.Name));
        await db.SaveChangesAsync(ct);
        return TypedResults.Created(
            $"/api/aw/product-inventories/by-key?productId={entity.ProductId}&locationId={entity.LocationId}",
            new CompositeKeyResponse(new Dictionary<string, object>
            {
                ["productId"] = entity.ProductId,
                ["locationId"] = entity.LocationId,
            }));
    }

    private static async Task<Results<Ok<CompositeKeyResponse>, NotFound, ValidationProblem>> UpdateAsync(
        [FromQuery] int productId, [FromQuery] short locationId,
        UpdateProductInventoryRequest request, IValidator<UpdateProductInventoryRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());

        var entity = await db.ProductInventories
            .FirstOrDefaultAsync(x => x.ProductId == productId && x.LocationId == locationId, ct);
        if (entity is null) return TypedResults.NotFound();

        var before = ProductInventoryAuditService.CaptureSnapshot(entity);
        request.ApplyTo(entity);
        db.ProductInventoryAuditLogs.Add(
            ProductInventoryAuditService.RecordUpdate(before, entity, user.Identity?.Name));
        await db.SaveChangesAsync(ct);
        return TypedResults.Ok(new CompositeKeyResponse(new Dictionary<string, object>
        {
            ["productId"] = entity.ProductId,
            ["locationId"] = entity.LocationId,
        }));
    }

    private static async Task<Results<NoContent, NotFound>> DeleteAsync(
        [FromQuery] int productId, [FromQuery] short locationId,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var entity = await db.ProductInventories
            .FirstOrDefaultAsync(x => x.ProductId == productId && x.LocationId == locationId, ct);
        if (entity is null) return TypedResults.NotFound();

        db.ProductInventoryAuditLogs.Add(ProductInventoryAuditService.RecordDelete(entity, user.Identity?.Name));
        db.ProductInventories.Remove(entity);
        await db.SaveChangesAsync(ct);
        return TypedResults.NoContent();
    }

    private static async Task<Ok<List<ProductInventoryAuditLogDto>>> HistoryAsync(
        ApplicationDbContext db,
        [FromQuery] int? productId = null,
        [FromQuery] short? locationId = null,
        CancellationToken ct = default)
    {
        var query = db.ProductInventoryAuditLogs.AsNoTracking();
        if (productId.HasValue) query = query.Where(a => a.ProductId == productId.Value);
        if (locationId.HasValue) query = query.Where(a => a.LocationId == locationId.Value);

        var rows = await query
            .OrderByDescending(a => a.ChangedDate).ThenByDescending(a => a.Id)
            .Select(a => a.ToDto())
            .ToListAsync(ct);
        return TypedResults.Ok(rows);
    }
}
