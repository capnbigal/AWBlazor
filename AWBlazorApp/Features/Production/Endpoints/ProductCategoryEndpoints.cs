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

public static class ProductCategoryEndpoints
{
    public static IEndpointRouteBuilder MapProductCategoryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/aw/product-categories")
            .WithTags("ProductCategories")
            .RequireAuthorization("ApiOrCookie");

        group.MapGet("/", ListAsync).WithName("ListProductCategories").WithSummary("List Production.ProductCategory rows.");

        group.MapIntIdCrud<ProductCategory, ProductCategoryDto, CreateProductCategoryRequest, UpdateProductCategoryRequest, ProductCategoryAuditLog, ProductCategoryAuditLogDto, ProductCategoryAuditService.Snapshot, int>(
            entityName: "ProductCategory",
            routePrefix: "/api/aw/product-categories",
            entitySet: db => db.ProductCategories,
            auditSet: db => db.ProductCategoryAuditLogs,
            idSelector: e => e.Id,
            auditIdSelector: a => a.ProductCategoryId,
            auditChangedDateSelector: a => a.ChangedDate,
            auditPrimaryKeySelector: a => a.Id,
            getId: e => e.Id,
            toDto: e => e.ToDto(),
            toEntity: r => r.ToEntity(),
            applyUpdate: (r, e) => r.ApplyTo(e),
            captureSnapshot: ProductCategoryAuditService.CaptureSnapshot,
            recordCreate: ProductCategoryAuditService.RecordCreate,
            recordUpdate: ProductCategoryAuditService.RecordUpdate,
            recordDelete: ProductCategoryAuditService.RecordDelete,
            auditToDto: a => a.ToDto());

        return app;
    }

    private static async Task<Ok<PagedResult<ProductCategoryDto>>> ListAsync(
        ApplicationDbContext db, [FromQuery] int skip = 0, [FromQuery] int take = 50,
        [FromQuery] string? name = null, CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 1000);
        var query = db.ProductCategories.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(name)) query = query.Where(x => x.Name.Contains(name));
        var total = await query.CountAsync(ct);
        var rows = await query.OrderBy(x => x.Id).Skip(skip).Take(take).Select(x => x.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<ProductCategoryDto>(rows, total, skip, take));
    }
}