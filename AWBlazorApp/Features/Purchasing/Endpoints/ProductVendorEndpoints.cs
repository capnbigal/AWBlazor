using System.Security.Claims;
using AWBlazorApp.Data;
using AWBlazorApp.Infrastructure.Persistence;
using AWBlazorApp.Shared.Models;
using AWBlazorApp.Features.Purchasing.Models;
using AWBlazorApp.Features.Purchasing.Audit;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.Purchasing.Endpoints;

public static class ProductVendorEndpoints
{
    public static IEndpointRouteBuilder MapProductVendorEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/aw/product-vendors")
            .WithTags("ProductVendors")
            .RequireAuthorization("ApiOrCookie");

        group.MapGet("/", ListAsync).WithName("ListProductVendors")
            .WithSummary("List Purchasing.ProductVendor rows. Composite PK = (ProductID, BusinessEntityID).");
        group.MapGet("/by-key", GetAsync).WithName("GetProductVendor");
        group.MapPost("/", CreateAsync).WithName("CreateProductVendor")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));
        group.MapPatch("/by-key", UpdateAsync).WithName("UpdateProductVendor")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));
        group.MapDelete("/by-key", DeleteAsync).WithName("DeleteProductVendor")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));
        group.MapGet("/history", HistoryAsync).WithName("ListProductVendorHistory");
        return app;
    }

    private static async Task<Ok<PagedResult<ProductVendorDto>>> ListAsync(
        ApplicationDbContext db, [FromQuery] int skip = 0, [FromQuery] int take = 50,
        [FromQuery] int? productId = null, [FromQuery] int? businessEntityId = null, CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 1000);
        var query = db.ProductVendors.AsNoTracking();
        if (productId.HasValue) query = query.Where(x => x.ProductId == productId.Value);
        if (businessEntityId.HasValue) query = query.Where(x => x.BusinessEntityId == businessEntityId.Value);
        var total = await query.CountAsync(ct);
        var rows = await query.OrderBy(x => x.ProductId).ThenBy(x => x.BusinessEntityId)
            .Skip(skip).Take(take).Select(x => x.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<ProductVendorDto>(rows, total, skip, take));
    }

    private static async Task<Results<Ok<ProductVendorDto>, NotFound>> GetAsync(
        [FromQuery] int productId, [FromQuery] int businessEntityId,
        ApplicationDbContext db, CancellationToken ct)
    {
        var row = await db.ProductVendors.AsNoTracking()
            .FirstOrDefaultAsync(x => x.ProductId == productId && x.BusinessEntityId == businessEntityId, ct);
        return row is null ? TypedResults.NotFound() : TypedResults.Ok(row.ToDto());
    }

    private static async Task<Results<Created<CompositeKeyResponse>, Conflict<string>, ValidationProblem>> CreateAsync(
        CreateProductVendorRequest request, IValidator<CreateProductVendorRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());

        if (await db.ProductVendors.AnyAsync(x =>
                x.ProductId == request.ProductId && x.BusinessEntityId == request.BusinessEntityId, ct))
        {
            return TypedResults.Conflict($"ProductVendor ({request.ProductId}, {request.BusinessEntityId}) already exists.");
        }

        var entity = request.ToEntity();
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        db.ProductVendors.Add(entity);
        await db.SaveChangesAsync(ct);
        db.ProductVendorAuditLogs.Add(ProductVendorAuditService.RecordCreate(entity, user.Identity?.Name));
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return TypedResults.Created(
            $"/api/aw/product-vendors/by-key?productId={entity.ProductId}&businessEntityId={entity.BusinessEntityId}",
            new CompositeKeyResponse(new Dictionary<string, object>
            {
                ["productId"] = entity.ProductId,
                ["businessEntityId"] = entity.BusinessEntityId,
            }));
    }

    private static async Task<Results<Ok<CompositeKeyResponse>, NotFound, ValidationProblem>> UpdateAsync(
        [FromQuery] int productId, [FromQuery] int businessEntityId,
        UpdateProductVendorRequest request, IValidator<UpdateProductVendorRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());

        var entity = await db.ProductVendors
            .FirstOrDefaultAsync(x => x.ProductId == productId && x.BusinessEntityId == businessEntityId, ct);
        if (entity is null) return TypedResults.NotFound();

        var before = ProductVendorAuditService.CaptureSnapshot(entity);
        request.ApplyTo(entity);
        db.ProductVendorAuditLogs.Add(
            ProductVendorAuditService.RecordUpdate(before, entity, user.Identity?.Name));
        await db.SaveChangesAsync(ct);
        return TypedResults.Ok(new CompositeKeyResponse(new Dictionary<string, object>
        {
            ["productId"] = entity.ProductId,
            ["businessEntityId"] = entity.BusinessEntityId,
        }));
    }

    private static async Task<Results<NoContent, NotFound>> DeleteAsync(
        [FromQuery] int productId, [FromQuery] int businessEntityId,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var entity = await db.ProductVendors
            .FirstOrDefaultAsync(x => x.ProductId == productId && x.BusinessEntityId == businessEntityId, ct);
        if (entity is null) return TypedResults.NotFound();

        db.ProductVendorAuditLogs.Add(ProductVendorAuditService.RecordDelete(entity, user.Identity?.Name));
        db.ProductVendors.Remove(entity);
        await db.SaveChangesAsync(ct);
        return TypedResults.NoContent();
    }

    private static async Task<Ok<List<ProductVendorAuditLogDto>>> HistoryAsync(
        ApplicationDbContext db,
        [FromQuery] int? productId = null,
        [FromQuery] int? businessEntityId = null,
        CancellationToken ct = default)
    {
        var query = db.ProductVendorAuditLogs.AsNoTracking();
        if (productId.HasValue) query = query.Where(a => a.ProductId == productId.Value);
        if (businessEntityId.HasValue) query = query.Where(a => a.BusinessEntityId == businessEntityId.Value);

        var rows = await query
            .OrderByDescending(a => a.ChangedDate).ThenByDescending(a => a.Id)
            .Select(a => a.ToDto())
            .ToListAsync(ct);
        return TypedResults.Ok(rows);
    }
}
