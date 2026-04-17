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

public static class ProductEndpoints
{
    public static IEndpointRouteBuilder MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/aw/products")
            .WithTags("Products")
            .RequireAuthorization("ApiOrCookie");

        group.MapGet("/", ListAsync).WithName("ListProducts").WithSummary("List Production.Product rows.");

        group.MapIntIdCrud<Product, ProductDto, CreateProductRequest, UpdateProductRequest, ProductAuditLog, ProductAuditLogDto, ProductAuditService.Snapshot, int>(
            entityName: "Product",
            routePrefix: "/api/aw/products",
            entitySet: db => db.Products,
            auditSet: db => db.ProductAuditLogs,
            idSelector: e => e.Id,
            auditIdSelector: a => a.ProductId,
            auditChangedDateSelector: a => a.ChangedDate,
            auditPrimaryKeySelector: a => a.Id,
            getId: e => e.Id,
            toDto: e => e.ToDto(),
            toEntity: r => r.ToEntity(),
            applyUpdate: (r, e) => r.ApplyTo(e),
            captureSnapshot: ProductAuditService.CaptureSnapshot,
            recordCreate: ProductAuditService.RecordCreate,
            recordUpdate: ProductAuditService.RecordUpdate,
            recordDelete: ProductAuditService.RecordDelete,
            auditToDto: a => a.ToDto());

        return app;
    }

    private static async Task<Ok<PagedResult<ProductDto>>> ListAsync(
        ApplicationDbContext db, [FromQuery] int skip = 0, [FromQuery] int take = 50,
        [FromQuery] string? name = null, [FromQuery] int? productSubcategoryId = null,
        CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 1000);
        var query = db.Products.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(name)) query = query.Where(x => x.Name.Contains(name));
        if (productSubcategoryId.HasValue) query = query.Where(x => x.ProductSubcategoryId == productSubcategoryId.Value);
        var total = await query.CountAsync(ct);
        var rows = await query.OrderBy(x => x.Name).Skip(skip).Take(take).Select(x => x.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<ProductDto>(rows, total, skip, take));
    }
}