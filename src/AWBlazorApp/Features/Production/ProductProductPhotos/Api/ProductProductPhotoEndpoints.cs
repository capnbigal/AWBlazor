using System.Security.Claims;
using AWBlazorApp.Features.Identity.Domain; using AWBlazorApp.Features.Admin.Permissions.Domain;
using AWBlazorApp.Infrastructure.Persistence;
using AWBlazorApp.Shared.Dtos;
using AWBlazorApp.Features.Production.Cultures.Dtos; using AWBlazorApp.Features.Production.Documents.Dtos; using AWBlazorApp.Features.Production.Illustrations.Dtos; using AWBlazorApp.Features.Production.Locations.Dtos; using AWBlazorApp.Features.Production.ProductCategories.Dtos; using AWBlazorApp.Features.Production.ProductCostHistories.Dtos; using AWBlazorApp.Features.Production.ProductDescriptions.Dtos; using AWBlazorApp.Features.Production.ProductDocuments.Dtos; using AWBlazorApp.Features.Production.ProductInventories.Dtos; using AWBlazorApp.Features.Production.ProductListPriceHistories.Dtos; using AWBlazorApp.Features.Production.ProductModels.Dtos; using AWBlazorApp.Features.Production.ProductModelIllustrations.Dtos; using AWBlazorApp.Features.Production.ProductModelProductDescriptionCultures.Dtos; using AWBlazorApp.Features.Production.ProductPhotos.Dtos; using AWBlazorApp.Features.Production.ProductProductPhotos.Dtos; using AWBlazorApp.Features.Production.ProductReviews.Dtos; using AWBlazorApp.Features.Production.Products.Dtos; using AWBlazorApp.Features.Production.ProductSubcategories.Dtos; using AWBlazorApp.Features.Production.ScrapReasons.Dtos; using AWBlazorApp.Features.Production.TransactionHistories.Dtos; using AWBlazorApp.Features.Production.TransactionHistoryArchives.Dtos; using AWBlazorApp.Features.Production.UnitMeasures.Dtos; using AWBlazorApp.Features.Production.WorkOrders.Dtos; using AWBlazorApp.Features.Production.WorkOrderRoutings.Dtos; 
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.Production.ProductProductPhotos.Api;

public static class ProductProductPhotoEndpoints
{
    public static IEndpointRouteBuilder MapProductProductPhotoEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/aw/product-product-photos")
            .WithTags("ProductProductPhotos")
            .RequireAuthorization("ApiOrCookie");

        group.MapGet("/", ListAsync).WithName("ListProductProductPhotos")
            .WithSummary("List Production.ProductProductPhoto rows. Composite PK = (ProductID, ProductPhotoID).");
        group.MapGet("/by-key", GetAsync).WithName("GetProductProductPhoto");
        group.MapPost("/", CreateAsync).WithName("CreateProductProductPhoto")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));
        group.MapPatch("/by-key", UpdateAsync).WithName("UpdateProductProductPhoto")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));
        group.MapDelete("/by-key", DeleteAsync).WithName("DeleteProductProductPhoto")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));
        group.MapGet("/history", HistoryAsync).WithName("ListProductProductPhotoHistory");
        return app;
    }

    private static async Task<Ok<PagedResult<ProductProductPhotoDto>>> ListAsync(
        ApplicationDbContext db, [FromQuery] int skip = 0, [FromQuery] int take = 50,
        [FromQuery] int? productId = null, CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 1000);
        var query = db.ProductProductPhotos.AsNoTracking();
        if (productId.HasValue) query = query.Where(x => x.ProductId == productId.Value);
        var total = await query.CountAsync(ct);
        var rows = await query.OrderBy(x => x.ProductId).ThenBy(x => x.ProductPhotoId)
            .Skip(skip).Take(take).Select(x => x.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<ProductProductPhotoDto>(rows, total, skip, take));
    }

    private static async Task<Results<Ok<ProductProductPhotoDto>, NotFound>> GetAsync(
        [FromQuery] int productId, [FromQuery] int productPhotoId,
        ApplicationDbContext db, CancellationToken ct)
    {
        var row = await db.ProductProductPhotos.AsNoTracking()
            .FirstOrDefaultAsync(x => x.ProductId == productId && x.ProductPhotoId == productPhotoId, ct);
        return row is null ? TypedResults.NotFound() : TypedResults.Ok(row.ToDto());
    }

    private static async Task<Results<Created<CompositeKeyResponse>, Conflict<string>, ValidationProblem>> CreateAsync(
        CreateProductProductPhotoRequest request, IValidator<CreateProductProductPhotoRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());

        if (await db.ProductProductPhotos.AnyAsync(x =>
                x.ProductId == request.ProductId && x.ProductPhotoId == request.ProductPhotoId, ct))
        {
            return TypedResults.Conflict($"Junction row ({request.ProductId}, {request.ProductPhotoId}) already exists.");
        }

        var entity = request.ToEntity();
        return TypedResults.Created(
            $"/api/aw/product-product-photos/by-key?productId={entity.ProductId}&productPhotoId={entity.ProductPhotoId}",
            new CompositeKeyResponse(new Dictionary<string, object>
            {
                ["productId"] = entity.ProductId,
                ["productPhotoId"] = entity.ProductPhotoId,
            }));
    }

    private static async Task<Results<Ok<CompositeKeyResponse>, NotFound, ValidationProblem>> UpdateAsync(
        [FromQuery] int productId, [FromQuery] int productPhotoId,
        UpdateProductProductPhotoRequest request, IValidator<UpdateProductProductPhotoRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());

        var entity = await db.ProductProductPhotos
            .FirstOrDefaultAsync(x => x.ProductId == productId && x.ProductPhotoId == productPhotoId, ct);
        if (entity is null) return TypedResults.NotFound();

        request.ApplyTo(entity);
        await db.SaveChangesAsync(ct);
        return TypedResults.Ok(new CompositeKeyResponse(new Dictionary<string, object>
        {
            ["productId"] = entity.ProductId,
            ["productPhotoId"] = entity.ProductPhotoId,
        }));
    }

    private static async Task<Results<NoContent, NotFound>> DeleteAsync(
        [FromQuery] int productId, [FromQuery] int productPhotoId,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var entity = await db.ProductProductPhotos
            .FirstOrDefaultAsync(x => x.ProductId == productId && x.ProductPhotoId == productPhotoId, ct);
        if (entity is null) return TypedResults.NotFound();

        db.ProductProductPhotos.Remove(entity);
        await db.SaveChangesAsync(ct);
        return TypedResults.NoContent();
    }

    private static async Task<Ok<List<AWBlazorApp.Shared.Audit.AuditLog>>> HistoryAsync(
        ApplicationDbContext db,
        CancellationToken ct = default)
    {
        var rows = await db.AuditLogs.AsNoTracking()
            .Where(a => a.EntityType == "ProductProductPhoto")
            .OrderByDescending(a => a.ChangedDate).ThenByDescending(a => a.Id)
            .ToListAsync(ct);
        return TypedResults.Ok(rows);
    }

}
