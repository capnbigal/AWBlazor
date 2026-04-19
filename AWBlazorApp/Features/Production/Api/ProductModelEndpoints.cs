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

public static class ProductModelEndpoints
{
    public static IEndpointRouteBuilder MapProductModelEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/aw/product-models")
            .WithTags("ProductModels")
            .RequireAuthorization("ApiOrCookie");

        group.MapGet("/", ListAsync).WithName("ListProductModels").WithSummary("List Production.ProductModel rows.");

        group.MapIntIdCrud<ProductModel, ProductModelDto, CreateProductModelRequest, UpdateProductModelRequest, ProductModelAuditLog, ProductModelAuditLogDto, ProductModelAuditService.Snapshot, int>(
            entityName: "ProductModel",
            routePrefix: "/api/aw/product-models",
            entitySet: db => db.ProductModels,
            auditSet: db => db.ProductModelAuditLogs,
            idSelector: e => e.Id,
            auditIdSelector: a => a.ProductModelId,
            auditChangedDateSelector: a => a.ChangedDate,
            auditPrimaryKeySelector: a => a.Id,
            getId: e => e.Id,
            toDto: e => e.ToDto(),
            toEntity: r => r.ToEntity(),
            applyUpdate: (r, e) => r.ApplyTo(e),
            captureSnapshot: ProductModelAuditService.CaptureSnapshot,
            recordCreate: ProductModelAuditService.RecordCreate,
            recordUpdate: ProductModelAuditService.RecordUpdate,
            recordDelete: ProductModelAuditService.RecordDelete,
            auditToDto: a => a.ToDto());

        return app;
    }

    private static async Task<Ok<PagedResult<ProductModelDto>>> ListAsync(
        ApplicationDbContext db, [FromQuery] int skip = 0, [FromQuery] int take = 50,
        [FromQuery] string? name = null, CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 1000);
        var query = db.ProductModels.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(name)) query = query.Where(x => x.Name.Contains(name));
        var total = await query.CountAsync(ct);
        var rows = await query.OrderBy(x => x.Name).Skip(skip).Take(take).Select(x => x.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<ProductModelDto>(rows, total, skip, take));
    }
}