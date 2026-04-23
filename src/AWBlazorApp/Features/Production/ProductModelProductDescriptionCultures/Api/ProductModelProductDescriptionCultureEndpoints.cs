using System.Security.Claims;
using AWBlazorApp.Features.Identity.Domain; using AWBlazorApp.Features.Admin.Permissions.Domain;
using AWBlazorApp.Infrastructure.Persistence;
using AWBlazorApp.Shared.Dtos;
using AWBlazorApp.Features.Production.Cultures.Dtos; using AWBlazorApp.Features.Production.Documents.Dtos; using AWBlazorApp.Features.Production.Illustrations.Dtos; using AWBlazorApp.Features.Production.Locations.Dtos; using AWBlazorApp.Features.Production.ProductCategories.Dtos; using AWBlazorApp.Features.Production.ProductCostHistories.Dtos; using AWBlazorApp.Features.Production.ProductDescriptions.Dtos; using AWBlazorApp.Features.Production.ProductDocuments.Dtos; using AWBlazorApp.Features.Production.ProductInventories.Dtos; using AWBlazorApp.Features.Production.ProductListPriceHistories.Dtos; using AWBlazorApp.Features.Production.ProductModels.Dtos; using AWBlazorApp.Features.Production.ProductModelIllustrations.Dtos; using AWBlazorApp.Features.Production.ProductModelProductDescriptionCultures.Dtos; using AWBlazorApp.Features.Production.ProductPhotos.Dtos; using AWBlazorApp.Features.Production.ProductProductPhotos.Dtos; using AWBlazorApp.Features.Production.ProductReviews.Dtos; using AWBlazorApp.Features.Production.Products.Dtos; using AWBlazorApp.Features.Production.ProductSubcategories.Dtos; using AWBlazorApp.Features.Production.ScrapReasons.Dtos; using AWBlazorApp.Features.Production.TransactionHistories.Dtos; using AWBlazorApp.Features.Production.TransactionHistoryArchives.Dtos; using AWBlazorApp.Features.Production.UnitMeasures.Dtos; using AWBlazorApp.Features.Production.WorkOrders.Dtos; using AWBlazorApp.Features.Production.WorkOrderRoutings.Dtos; 
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.Production.ProductModelProductDescriptionCultures.Api;

public static class ProductModelProductDescriptionCultureEndpoints
{
    public static IEndpointRouteBuilder MapProductModelProductDescriptionCultureEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/aw/product-model-product-description-cultures")
            .WithTags("ProductModelProductDescriptionCultures")
            .RequireAuthorization("ApiOrCookie");

        group.MapGet("/", ListAsync).WithName("ListProductModelProductDescriptionCultures")
            .WithSummary("List Production.ProductModelProductDescriptionCulture rows. 3-col composite PK = (ProductModelID, ProductDescriptionID, CultureID).");
        group.MapGet("/by-key", GetAsync).WithName("GetProductModelProductDescriptionCulture");
        group.MapPost("/", CreateAsync).WithName("CreateProductModelProductDescriptionCulture")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));
        group.MapPatch("/by-key", UpdateAsync).WithName("UpdateProductModelProductDescriptionCulture")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));
        group.MapDelete("/by-key", DeleteAsync).WithName("DeleteProductModelProductDescriptionCulture")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));
        group.MapGet("/history", HistoryAsync).WithName("ListProductModelProductDescriptionCultureHistory");
        return app;
    }

    private static async Task<Ok<PagedResult<ProductModelProductDescriptionCultureDto>>> ListAsync(
        ApplicationDbContext db, [FromQuery] int skip = 0, [FromQuery] int take = 50,
        [FromQuery] int? productModelId = null, CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 1000);
        var query = db.ProductModelProductDescriptionCultures.AsNoTracking();
        if (productModelId.HasValue) query = query.Where(x => x.ProductModelId == productModelId.Value);
        var total = await query.CountAsync(ct);
        var rows = await query.OrderBy(x => x.ProductModelId).ThenBy(x => x.ProductDescriptionId).ThenBy(x => x.CultureId)
            .Skip(skip).Take(take).Select(x => x.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<ProductModelProductDescriptionCultureDto>(rows, total, skip, take));
    }

    private static async Task<Results<Ok<ProductModelProductDescriptionCultureDto>, NotFound>> GetAsync(
        [FromQuery] int productModelId, [FromQuery] int productDescriptionId, [FromQuery] string cultureId,
        ApplicationDbContext db, CancellationToken ct)
    {
        var row = await db.ProductModelProductDescriptionCultures.AsNoTracking()
            .FirstOrDefaultAsync(x => x.ProductModelId == productModelId
                && x.ProductDescriptionId == productDescriptionId
                && x.CultureId == cultureId, ct);
        return row is null ? TypedResults.NotFound() : TypedResults.Ok(row.ToDto());
    }

    private static async Task<Results<Created<CompositeKeyResponse>, Conflict<string>, ValidationProblem>> CreateAsync(
        CreateProductModelProductDescriptionCultureRequest request,
        IValidator<CreateProductModelProductDescriptionCultureRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());

        if (await db.ProductModelProductDescriptionCultures.AnyAsync(x =>
                x.ProductModelId == request.ProductModelId
                && x.ProductDescriptionId == request.ProductDescriptionId
                && x.CultureId == request.CultureId, ct))
        {
            return TypedResults.Conflict($"Junction row ({request.ProductModelId}, {request.ProductDescriptionId}, {request.CultureId}) already exists.");
        }

        var entity = request.ToEntity();
        return TypedResults.Created(
            $"/api/aw/product-model-product-description-cultures/by-key?productModelId={entity.ProductModelId}&productDescriptionId={entity.ProductDescriptionId}&cultureId={entity.CultureId}",
            new CompositeKeyResponse(new Dictionary<string, object>
            {
                ["productModelId"] = entity.ProductModelId,
                ["productDescriptionId"] = entity.ProductDescriptionId,
                ["cultureId"] = entity.CultureId,
            }));
    }

    private static async Task<Results<Ok<CompositeKeyResponse>, NotFound, ValidationProblem>> UpdateAsync(
        [FromQuery] int productModelId, [FromQuery] int productDescriptionId, [FromQuery] string cultureId,
        UpdateProductModelProductDescriptionCultureRequest request,
        IValidator<UpdateProductModelProductDescriptionCultureRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());

        var entity = await db.ProductModelProductDescriptionCultures
            .FirstOrDefaultAsync(x => x.ProductModelId == productModelId
                && x.ProductDescriptionId == productDescriptionId
                && x.CultureId == cultureId, ct);
        if (entity is null) return TypedResults.NotFound();

        request.ApplyTo(entity);
        await db.SaveChangesAsync(ct);
        return TypedResults.Ok(new CompositeKeyResponse(new Dictionary<string, object>
        {
            ["productModelId"] = entity.ProductModelId,
            ["productDescriptionId"] = entity.ProductDescriptionId,
            ["cultureId"] = entity.CultureId,
        }));
    }

    private static async Task<Results<NoContent, NotFound>> DeleteAsync(
        [FromQuery] int productModelId, [FromQuery] int productDescriptionId, [FromQuery] string cultureId,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var entity = await db.ProductModelProductDescriptionCultures
            .FirstOrDefaultAsync(x => x.ProductModelId == productModelId
                && x.ProductDescriptionId == productDescriptionId
                && x.CultureId == cultureId, ct);
        if (entity is null) return TypedResults.NotFound();

        db.ProductModelProductDescriptionCultures.Remove(entity);
        await db.SaveChangesAsync(ct);
        return TypedResults.NoContent();
    }

    private static async Task<Ok<List<AWBlazorApp.Shared.Audit.AuditLog>>> HistoryAsync(
        ApplicationDbContext db,
        CancellationToken ct = default)
    {
        var rows = await db.AuditLogs.AsNoTracking()
            .Where(a => a.EntityType == "ProductModelProductDescriptionCulture")
            .OrderByDescending(a => a.ChangedDate).ThenByDescending(a => a.Id)
            .ToListAsync(ct);
        return TypedResults.Ok(rows);
    }

}
