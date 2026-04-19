using System.Security.Claims;
using AWBlazorApp.Features.Identity.Domain; using AWBlazorApp.Features.Admin.Permissions.Domain;
using AWBlazorApp.Infrastructure.Persistence;
using AWBlazorApp.Shared.Dtos;
using AWBlazorApp.Features.Production.Dtos; using AWBlazorApp.Features.Production.Cultures.Dtos; using AWBlazorApp.Features.Production.Documents.Dtos; using AWBlazorApp.Features.Production.Illustrations.Dtos; using AWBlazorApp.Features.Production.Locations.Dtos; using AWBlazorApp.Features.Production.ProductCategories.Dtos; using AWBlazorApp.Features.Production.ProductCostHistories.Dtos; using AWBlazorApp.Features.Production.ProductDescriptions.Dtos; using AWBlazorApp.Features.Production.ProductDocuments.Dtos; using AWBlazorApp.Features.Production.ProductInventories.Dtos; using AWBlazorApp.Features.Production.ProductListPriceHistories.Dtos; using AWBlazorApp.Features.Production.ProductModels.Dtos; using AWBlazorApp.Features.Production.ProductModelIllustrations.Dtos; using AWBlazorApp.Features.Production.ProductModelProductDescriptionCultures.Dtos; using AWBlazorApp.Features.Production.ProductPhotos.Dtos; using AWBlazorApp.Features.Production.ProductProductPhotos.Dtos; using AWBlazorApp.Features.Production.ProductReviews.Dtos; using AWBlazorApp.Features.Production.Products.Dtos; using AWBlazorApp.Features.Production.ProductSubcategories.Dtos; using AWBlazorApp.Features.Production.ScrapReasons.Dtos; using AWBlazorApp.Features.Production.TransactionHistories.Dtos; using AWBlazorApp.Features.Production.TransactionHistoryArchives.Dtos; using AWBlazorApp.Features.Production.UnitMeasures.Dtos; using AWBlazorApp.Features.Production.WorkOrders.Dtos; using AWBlazorApp.Features.Production.WorkOrderRoutings.Dtos; 
using AWBlazorApp.Features.Production.Audit; using AWBlazorApp.Features.Production.Cultures.Application.Services; using AWBlazorApp.Features.Production.Documents.Application.Services; using AWBlazorApp.Features.Production.Illustrations.Application.Services; using AWBlazorApp.Features.Production.Locations.Application.Services; using AWBlazorApp.Features.Production.ProductCategories.Application.Services; using AWBlazorApp.Features.Production.ProductCostHistories.Application.Services; using AWBlazorApp.Features.Production.ProductDescriptions.Application.Services; using AWBlazorApp.Features.Production.ProductDocuments.Application.Services; using AWBlazorApp.Features.Production.ProductInventories.Application.Services; using AWBlazorApp.Features.Production.ProductListPriceHistories.Application.Services; using AWBlazorApp.Features.Production.ProductModels.Application.Services; using AWBlazorApp.Features.Production.ProductModelIllustrations.Application.Services; using AWBlazorApp.Features.Production.ProductModelProductDescriptionCultures.Application.Services; using AWBlazorApp.Features.Production.ProductPhotos.Application.Services; using AWBlazorApp.Features.Production.ProductProductPhotos.Application.Services; using AWBlazorApp.Features.Production.ProductReviews.Application.Services; using AWBlazorApp.Features.Production.Products.Application.Services; using AWBlazorApp.Features.Production.ProductSubcategories.Application.Services; using AWBlazorApp.Features.Production.ScrapReasons.Application.Services; using AWBlazorApp.Features.Production.TransactionHistories.Application.Services; using AWBlazorApp.Features.Production.TransactionHistoryArchives.Application.Services; using AWBlazorApp.Features.Production.UnitMeasures.Application.Services; using AWBlazorApp.Features.Production.WorkOrders.Application.Services; using AWBlazorApp.Features.Production.WorkOrderRoutings.Application.Services; 
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.Production.ProductCostHistories.Api;

public static class ProductCostHistoryEndpoints
{
    public static IEndpointRouteBuilder MapProductCostHistoryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/aw/product-cost-histories")
            .WithTags("ProductCostHistories")
            .RequireAuthorization("ApiOrCookie");

        group.MapGet("/", ListAsync).WithName("ListProductCostHistories")
            .WithSummary("List Production.ProductCostHistory rows. Composite PK = (ProductID, StartDate).");
        group.MapGet("/by-key", GetAsync).WithName("GetProductCostHistory");
        group.MapPost("/", CreateAsync).WithName("CreateProductCostHistory")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));
        group.MapPatch("/by-key", UpdateAsync).WithName("UpdateProductCostHistory")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));
        group.MapDelete("/by-key", DeleteAsync).WithName("DeleteProductCostHistory")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));
        group.MapGet("/history", HistoryAsync).WithName("ListProductCostHistoryHistory");
        return app;
    }

    private static async Task<Ok<PagedResult<ProductCostHistoryDto>>> ListAsync(
        ApplicationDbContext db, [FromQuery] int skip = 0, [FromQuery] int take = 50,
        [FromQuery] int? productId = null, CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 1000);
        var query = db.ProductCostHistories.AsNoTracking();
        if (productId.HasValue) query = query.Where(x => x.ProductId == productId.Value);
        var total = await query.CountAsync(ct);
        var rows = await query.OrderBy(x => x.ProductId).ThenByDescending(x => x.StartDate)
            .Skip(skip).Take(take).Select(x => x.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<ProductCostHistoryDto>(rows, total, skip, take));
    }

    private static async Task<Results<Ok<ProductCostHistoryDto>, NotFound>> GetAsync(
        [FromQuery] int productId, [FromQuery] DateTime startDate,
        ApplicationDbContext db, CancellationToken ct)
    {
        var row = await db.ProductCostHistories.AsNoTracking()
            .FirstOrDefaultAsync(x => x.ProductId == productId && x.StartDate == startDate, ct);
        return row is null ? TypedResults.NotFound() : TypedResults.Ok(row.ToDto());
    }

    private static async Task<Results<Created<CompositeKeyResponse>, Conflict<string>, ValidationProblem>> CreateAsync(
        CreateProductCostHistoryRequest request, IValidator<CreateProductCostHistoryRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());

        if (await db.ProductCostHistories.AnyAsync(x =>
                x.ProductId == request.ProductId && x.StartDate == request.StartDate, ct))
        {
            return TypedResults.Conflict($"Cost row for ({request.ProductId}, {request.StartDate:O}) already exists.");
        }

        var entity = request.ToEntity();
        await db.AddWithAuditAsync(entity, e => ProductCostHistoryAuditService.RecordCreate(e, user.Identity?.Name), ct);
        return TypedResults.Created(
            $"/api/aw/product-cost-histories/by-key?productId={entity.ProductId}&startDate={entity.StartDate:O}",
            new CompositeKeyResponse(new Dictionary<string, object>
            {
                ["productId"] = entity.ProductId,
                ["startDate"] = entity.StartDate,
            }));
    }

    private static async Task<Results<Ok<CompositeKeyResponse>, NotFound, ValidationProblem>> UpdateAsync(
        [FromQuery] int productId, [FromQuery] DateTime startDate,
        UpdateProductCostHistoryRequest request, IValidator<UpdateProductCostHistoryRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());

        var entity = await db.ProductCostHistories
            .FirstOrDefaultAsync(x => x.ProductId == productId && x.StartDate == startDate, ct);
        if (entity is null) return TypedResults.NotFound();

        var before = ProductCostHistoryAuditService.CaptureSnapshot(entity);
        request.ApplyTo(entity);
        db.ProductCostHistoryAuditLogs.Add(
            ProductCostHistoryAuditService.RecordUpdate(before, entity, user.Identity?.Name));
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
        var entity = await db.ProductCostHistories
            .FirstOrDefaultAsync(x => x.ProductId == productId && x.StartDate == startDate, ct);
        if (entity is null) return TypedResults.NotFound();

        db.ProductCostHistoryAuditLogs.Add(ProductCostHistoryAuditService.RecordDelete(entity, user.Identity?.Name));
        db.ProductCostHistories.Remove(entity);
        await db.SaveChangesAsync(ct);
        return TypedResults.NoContent();
    }

    private static async Task<Ok<List<ProductCostHistoryAuditLogDto>>> HistoryAsync(
        ApplicationDbContext db,
        [FromQuery] int? productId = null,
        [FromQuery] DateTime? startDate = null,
        CancellationToken ct = default)
    {
        var query = db.ProductCostHistoryAuditLogs.AsNoTracking();
        if (productId.HasValue) query = query.Where(a => a.ProductId == productId.Value);
        if (startDate.HasValue) query = query.Where(a => a.StartDate == startDate.Value);

        var rows = await query
            .OrderByDescending(a => a.ChangedDate).ThenByDescending(a => a.Id)
            .Select(a => a.ToDto())
            .ToListAsync(ct);
        return TypedResults.Ok(rows);
    }
}
