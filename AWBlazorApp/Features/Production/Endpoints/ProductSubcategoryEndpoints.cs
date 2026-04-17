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

public static class ProductSubcategoryEndpoints
{
    public static IEndpointRouteBuilder MapProductSubcategoryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/aw/product-subcategories")
            .WithTags("ProductSubcategories")
            .RequireAuthorization("ApiOrCookie");

        group.MapGet("/", ListAsync).WithName("ListProductSubcategories").WithSummary("List Production.ProductSubcategory rows.");

        group.MapIntIdCrud<ProductSubcategory, ProductSubcategoryDto, CreateProductSubcategoryRequest, UpdateProductSubcategoryRequest, ProductSubcategoryAuditLog, ProductSubcategoryAuditLogDto, ProductSubcategoryAuditService.Snapshot, int>(
            entityName: "ProductSubcategory",
            routePrefix: "/api/aw/product-subcategories",
            entitySet: db => db.ProductSubcategories,
            auditSet: db => db.ProductSubcategoryAuditLogs,
            idSelector: e => e.Id,
            auditIdSelector: a => a.ProductSubcategoryId,
            auditChangedDateSelector: a => a.ChangedDate,
            auditPrimaryKeySelector: a => a.Id,
            getId: e => e.Id,
            toDto: e => e.ToDto(),
            toEntity: r => r.ToEntity(),
            applyUpdate: (r, e) => r.ApplyTo(e),
            captureSnapshot: ProductSubcategoryAuditService.CaptureSnapshot,
            recordCreate: ProductSubcategoryAuditService.RecordCreate,
            recordUpdate: ProductSubcategoryAuditService.RecordUpdate,
            recordDelete: ProductSubcategoryAuditService.RecordDelete,
            auditToDto: a => a.ToDto());

        return app;
    }

    private static async Task<Ok<PagedResult<ProductSubcategoryDto>>> ListAsync(
        ApplicationDbContext db, [FromQuery] int skip = 0, [FromQuery] int take = 50,
        [FromQuery] string? name = null, [FromQuery] int? productCategoryId = null, CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 1000);
        var query = db.ProductSubcategories.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(name)) query = query.Where(x => x.Name.Contains(name));
        if (productCategoryId.HasValue) query = query.Where(x => x.ProductCategoryId == productCategoryId.Value);
        var total = await query.CountAsync(ct);
        var rows = await query.OrderBy(x => x.Id).Skip(skip).Take(take).Select(x => x.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<ProductSubcategoryDto>(rows, total, skip, take));
    }
}