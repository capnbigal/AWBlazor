using AWBlazorApp.Features.Identity.Domain; using AWBlazorApp.Features.Admin.Permissions.Domain;
using AWBlazorApp.Infrastructure.Persistence;
using AWBlazorApp.Shared.Api;
using AWBlazorApp.Shared.Dtos;
using AWBlazorApp.Features.Production.Audit; using AWBlazorApp.Features.Production.Cultures.Application.Services; using AWBlazorApp.Features.Production.Documents.Application.Services; using AWBlazorApp.Features.Production.Illustrations.Application.Services; using AWBlazorApp.Features.Production.Locations.Application.Services; using AWBlazorApp.Features.Production.ProductCategories.Application.Services; using AWBlazorApp.Features.Production.ProductCostHistories.Application.Services; using AWBlazorApp.Features.Production.ProductDescriptions.Application.Services; using AWBlazorApp.Features.Production.ProductDocuments.Application.Services; using AWBlazorApp.Features.Production.ProductInventories.Application.Services; using AWBlazorApp.Features.Production.ProductListPriceHistories.Application.Services; using AWBlazorApp.Features.Production.ProductModels.Application.Services; using AWBlazorApp.Features.Production.ProductModelIllustrations.Application.Services; using AWBlazorApp.Features.Production.ProductModelProductDescriptionCultures.Application.Services; using AWBlazorApp.Features.Production.ProductPhotos.Application.Services; using AWBlazorApp.Features.Production.ProductProductPhotos.Application.Services; using AWBlazorApp.Features.Production.ProductReviews.Application.Services; using AWBlazorApp.Features.Production.Products.Application.Services; using AWBlazorApp.Features.Production.ProductSubcategories.Application.Services; using AWBlazorApp.Features.Production.ScrapReasons.Application.Services; using AWBlazorApp.Features.Production.TransactionHistories.Application.Services; using AWBlazorApp.Features.Production.TransactionHistoryArchives.Application.Services; using AWBlazorApp.Features.Production.UnitMeasures.Application.Services; using AWBlazorApp.Features.Production.WorkOrders.Application.Services; using AWBlazorApp.Features.Production.WorkOrderRoutings.Application.Services; 
using AWBlazorApp.Features.Production.Domain; using AWBlazorApp.Features.Production.Cultures.Domain; using AWBlazorApp.Features.Production.Documents.Domain; using AWBlazorApp.Features.Production.Illustrations.Domain; using AWBlazorApp.Features.Production.Locations.Domain; using AWBlazorApp.Features.Production.ProductCategories.Domain; using AWBlazorApp.Features.Production.ProductCostHistories.Domain; using AWBlazorApp.Features.Production.ProductDescriptions.Domain; using AWBlazorApp.Features.Production.ProductDocuments.Domain; using AWBlazorApp.Features.Production.ProductInventories.Domain; using AWBlazorApp.Features.Production.ProductListPriceHistories.Domain; using AWBlazorApp.Features.Production.ProductModels.Domain; using AWBlazorApp.Features.Production.ProductModelIllustrations.Domain; using AWBlazorApp.Features.Production.ProductModelProductDescriptionCultures.Domain; using AWBlazorApp.Features.Production.ProductPhotos.Domain; using AWBlazorApp.Features.Production.ProductProductPhotos.Domain; using AWBlazorApp.Features.Production.ProductReviews.Domain; using AWBlazorApp.Features.Production.Products.Domain; using AWBlazorApp.Features.Production.ProductSubcategories.Domain; using AWBlazorApp.Features.Production.ScrapReasons.Domain; using AWBlazorApp.Features.Production.TransactionHistories.Domain; using AWBlazorApp.Features.Production.TransactionHistoryArchives.Domain; using AWBlazorApp.Features.Production.UnitMeasures.Domain; using AWBlazorApp.Features.Production.WorkOrders.Domain; using AWBlazorApp.Features.Production.WorkOrderRoutings.Domain; 
using AWBlazorApp.Features.Production.Dtos; using AWBlazorApp.Features.Production.Cultures.Dtos; using AWBlazorApp.Features.Production.Documents.Dtos; using AWBlazorApp.Features.Production.Illustrations.Dtos; using AWBlazorApp.Features.Production.Locations.Dtos; using AWBlazorApp.Features.Production.ProductCategories.Dtos; using AWBlazorApp.Features.Production.ProductCostHistories.Dtos; using AWBlazorApp.Features.Production.ProductDescriptions.Dtos; using AWBlazorApp.Features.Production.ProductDocuments.Dtos; using AWBlazorApp.Features.Production.ProductInventories.Dtos; using AWBlazorApp.Features.Production.ProductListPriceHistories.Dtos; using AWBlazorApp.Features.Production.ProductModels.Dtos; using AWBlazorApp.Features.Production.ProductModelIllustrations.Dtos; using AWBlazorApp.Features.Production.ProductModelProductDescriptionCultures.Dtos; using AWBlazorApp.Features.Production.ProductPhotos.Dtos; using AWBlazorApp.Features.Production.ProductProductPhotos.Dtos; using AWBlazorApp.Features.Production.ProductReviews.Dtos; using AWBlazorApp.Features.Production.Products.Dtos; using AWBlazorApp.Features.Production.ProductSubcategories.Dtos; using AWBlazorApp.Features.Production.ScrapReasons.Dtos; using AWBlazorApp.Features.Production.TransactionHistories.Dtos; using AWBlazorApp.Features.Production.TransactionHistoryArchives.Dtos; using AWBlazorApp.Features.Production.UnitMeasures.Dtos; using AWBlazorApp.Features.Production.WorkOrders.Dtos; using AWBlazorApp.Features.Production.WorkOrderRoutings.Dtos; 
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.Production.ProductPhotos.Api;

public static class ProductPhotoEndpoints
{
    public static IEndpointRouteBuilder MapProductPhotoEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/aw/product-photos")
            .WithTags("ProductPhotos")
            .RequireAuthorization("ApiOrCookie");

        group.MapGet("/", ListAsync).WithName("ListProductPhotos").WithSummary("List Production.ProductPhoto rows. Image bytes are not exposed.");

        group.MapIntIdCrud<ProductPhoto, ProductPhotoDto, CreateProductPhotoRequest, UpdateProductPhotoRequest, ProductPhotoAuditLog, ProductPhotoAuditLogDto, ProductPhotoAuditService.Snapshot, int>(
            entityName: "ProductPhoto",
            routePrefix: "/api/aw/product-photos",
            entitySet: db => db.ProductPhotos,
            auditSet: db => db.ProductPhotoAuditLogs,
            idSelector: e => e.Id,
            auditIdSelector: a => a.ProductPhotoId,
            auditChangedDateSelector: a => a.ChangedDate,
            auditPrimaryKeySelector: a => a.Id,
            getId: e => e.Id,
            toDto: e => e.ToDto(),
            toEntity: r => r.ToEntity(),
            applyUpdate: (r, e) => r.ApplyTo(e),
            captureSnapshot: ProductPhotoAuditService.CaptureSnapshot,
            recordCreate: ProductPhotoAuditService.RecordCreate,
            recordUpdate: ProductPhotoAuditService.RecordUpdate,
            recordDelete: ProductPhotoAuditService.RecordDelete,
            auditToDto: a => a.ToDto());

        return app;
    }

    private static async Task<Ok<PagedResult<ProductPhotoDto>>> ListAsync(
        ApplicationDbContext db, [FromQuery] int skip = 0, [FromQuery] int take = 50,
        CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 1000);
        var query = db.ProductPhotos.AsNoTracking();
        var total = await query.CountAsync(ct);
        // Don't pull image bytes back for the list — project to DTO inside the SQL projection.
        var rows = await query.OrderBy(x => x.Id).Skip(skip).Take(take)
            .Select(x => new ProductPhotoDto(
                x.Id, x.ThumbnailPhotoFileName, x.LargePhotoFileName,
                x.ThumbNailPhoto != null && x.ThumbNailPhoto.Length > 0,
                x.LargePhoto != null && x.LargePhoto.Length > 0,
                x.ModifiedDate))
            .ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<ProductPhotoDto>(rows, total, skip, take));
    }
}