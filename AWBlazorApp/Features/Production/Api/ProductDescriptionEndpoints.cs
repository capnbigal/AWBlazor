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

public static class ProductDescriptionEndpoints
{
    public static IEndpointRouteBuilder MapProductDescriptionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/aw/product-descriptions")
            .WithTags("ProductDescriptions")
            .RequireAuthorization("ApiOrCookie");

        group.MapGet("/", ListAsync).WithName("ListProductDescriptions").WithSummary("List Production.ProductDescription rows.");

        group.MapIntIdCrud<ProductDescription, ProductDescriptionDto, CreateProductDescriptionRequest, UpdateProductDescriptionRequest, ProductDescriptionAuditLog, ProductDescriptionAuditLogDto, ProductDescriptionAuditService.Snapshot, int>(
            entityName: "ProductDescription",
            routePrefix: "/api/aw/product-descriptions",
            entitySet: db => db.ProductDescriptions,
            auditSet: db => db.ProductDescriptionAuditLogs,
            idSelector: e => e.Id,
            auditIdSelector: a => a.ProductDescriptionId,
            auditChangedDateSelector: a => a.ChangedDate,
            auditPrimaryKeySelector: a => a.Id,
            getId: e => e.Id,
            toDto: e => e.ToDto(),
            toEntity: r => r.ToEntity(),
            applyUpdate: (r, e) => r.ApplyTo(e),
            captureSnapshot: ProductDescriptionAuditService.CaptureSnapshot,
            recordCreate: ProductDescriptionAuditService.RecordCreate,
            recordUpdate: ProductDescriptionAuditService.RecordUpdate,
            recordDelete: ProductDescriptionAuditService.RecordDelete,
            auditToDto: a => a.ToDto());

        return app;
    }

    private static async Task<Ok<PagedResult<ProductDescriptionDto>>> ListAsync(
        ApplicationDbContext db, [FromQuery] int skip = 0, [FromQuery] int take = 50,
        [FromQuery] string? contains = null, CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 1000);
        var query = db.ProductDescriptions.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(contains)) query = query.Where(x => x.Description.Contains(contains));
        var total = await query.CountAsync(ct);
        var rows = await query.OrderBy(x => x.Id).Skip(skip).Take(take).Select(x => x.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<ProductDescriptionDto>(rows, total, skip, take));
    }
}