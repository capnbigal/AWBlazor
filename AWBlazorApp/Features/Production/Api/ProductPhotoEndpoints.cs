using AWBlazorApp.Features.Identity.Domain; using AWBlazorApp.Features.Admin.Permissions.Domain;
using AWBlazorApp.Infrastructure.Persistence;
using AWBlazorApp.Shared.Api;
using AWBlazorApp.Shared.Dtos;
using AWBlazorApp.Features.Production.Audit;
using AWBlazorApp.Features.Production.Domain;
using AWBlazorApp.Features.Production.Dtos;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.Production.Api;

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