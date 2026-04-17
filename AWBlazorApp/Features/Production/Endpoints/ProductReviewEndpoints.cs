using AWBlazorApp.Data;
using AWBlazorApp.Infrastructure.Persistence;
using AWBlazorApp.Shared.Endpoints;
using AWBlazorApp.Shared.Models;
using AWBlazorApp.Features.Production.Audit;
using AWBlazorApp.Features.Production.Domain;
using AWBlazorApp.Features.Production.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.Production.Endpoints;

public static class ProductReviewEndpoints
{
    public static IEndpointRouteBuilder MapProductReviewEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/aw/product-reviews")
            .WithTags("ProductReviews")
            .RequireAuthorization("ApiOrCookie");

        group.MapGet("/", ListAsync).WithName("ListProductReviews").WithSummary("List Production.ProductReview rows.");

        group.MapIntIdCrud<ProductReview, ProductReviewDto, CreateProductReviewRequest, UpdateProductReviewRequest, ProductReviewAuditLog, ProductReviewAuditLogDto, ProductReviewAuditService.Snapshot, int>(
            entityName: "ProductReview",
            routePrefix: "/api/aw/product-reviews",
            entitySet: db => db.ProductReviews,
            auditSet: db => db.ProductReviewAuditLogs,
            idSelector: e => e.Id,
            auditIdSelector: a => a.ProductReviewId,
            auditChangedDateSelector: a => a.ChangedDate,
            auditPrimaryKeySelector: a => a.Id,
            getId: e => e.Id,
            toDto: e => e.ToDto(),
            toEntity: r => r.ToEntity(),
            applyUpdate: (r, e) => r.ApplyTo(e),
            captureSnapshot: ProductReviewAuditService.CaptureSnapshot,
            recordCreate: ProductReviewAuditService.RecordCreate,
            recordUpdate: ProductReviewAuditService.RecordUpdate,
            recordDelete: ProductReviewAuditService.RecordDelete,
            auditToDto: a => a.ToDto());

        return app;
    }

    private static async Task<Ok<PagedResult<ProductReviewDto>>> ListAsync(
        ApplicationDbContext db, [FromQuery] int skip = 0, [FromQuery] int take = 50,
        [FromQuery] int? productId = null, [FromQuery] int? minRating = null,
        CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 1000);
        var query = db.ProductReviews.AsNoTracking();
        if (productId.HasValue) query = query.Where(x => x.ProductId == productId.Value);
        if (minRating.HasValue) query = query.Where(x => x.Rating >= minRating.Value);
        var total = await query.CountAsync(ct);
        var rows = await query.OrderByDescending(x => x.ReviewDate).ThenBy(x => x.Id)
            .Skip(skip).Take(take).Select(x => x.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<ProductReviewDto>(rows, total, skip, take));
    }
}