using AWBlazorApp.Features.Identity.Domain; using AWBlazorApp.Features.Admin.Permissions.Domain;
using AWBlazorApp.Infrastructure.Persistence;
using AWBlazorApp.Shared.Api;
using AWBlazorApp.Shared.Dtos;
using AWBlazorApp.Features.Enterprise.Assets.Application.Services; using AWBlazorApp.Features.Enterprise.CostCenters.Application.Services; using AWBlazorApp.Features.Enterprise.OrgUnits.Application.Services; using AWBlazorApp.Features.Enterprise.Organizations.Application.Services; using AWBlazorApp.Features.Enterprise.ProductLines.Application.Services; using AWBlazorApp.Features.Enterprise.Stations.Application.Services; 
using AWBlazorApp.Features.Enterprise.Assets.Domain; using AWBlazorApp.Features.Enterprise.CostCenters.Domain; using AWBlazorApp.Features.Enterprise.OrgUnits.Domain; using AWBlazorApp.Features.Enterprise.Organizations.Domain; using AWBlazorApp.Features.Enterprise.ProductLines.Domain; using AWBlazorApp.Features.Enterprise.Stations.Domain; 
using AWBlazorApp.Features.Enterprise.Assets.Dtos; using AWBlazorApp.Features.Enterprise.CostCenters.Dtos; using AWBlazorApp.Features.Enterprise.OrgUnits.Dtos; using AWBlazorApp.Features.Enterprise.Organizations.Dtos; using AWBlazorApp.Features.Enterprise.ProductLines.Dtos; using AWBlazorApp.Features.Enterprise.Stations.Dtos; 
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.Enterprise.ProductLines.Api;

public static class ProductLineEndpoints
{
    public static IEndpointRouteBuilder MapProductLineEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/product-lines")
            .WithTags("ProductLines")
            .RequireAuthorization("ApiOrCookie");

        group.MapGet("/", ListAsync).WithName("ListProductLines").WithSummary("List org.ProductLine rows.");

        group.MapIntIdCrud<ProductLine, ProductLineDto, CreateProductLineRequest, UpdateProductLineRequest, ProductLineAuditLog, ProductLineAuditLogDto, ProductLineAuditService.Snapshot, int>(
            entityName: "ProductLine",
            routePrefix: "/api/product-lines",
            entitySet: db => db.ProductLines,
            auditSet: db => db.ProductLineAuditLogs,
            idSelector: e => e.Id,
            auditIdSelector: a => a.ProductLineId,
            auditChangedDateSelector: a => a.ChangedDate,
            auditPrimaryKeySelector: a => a.Id,
            getId: e => e.Id,
            toDto: e => e.ToDto(),
            toEntity: r => r.ToEntity(),
            applyUpdate: (r, e) => r.ApplyTo(e),
            captureSnapshot: ProductLineAuditService.CaptureSnapshot,
            recordCreate: ProductLineAuditService.RecordCreate,
            recordUpdate: ProductLineAuditService.RecordUpdate,
            recordDelete: ProductLineAuditService.RecordDelete,
            auditToDto: a => a.ToDto());

        return app;
    }

    private static async Task<Ok<PagedResult<ProductLineDto>>> ListAsync(
        ApplicationDbContext db,
        [FromQuery] int skip = 0, [FromQuery] int take = 50,
        [FromQuery] int? organizationId = null, [FromQuery] string? code = null,
        [FromQuery] string? name = null, [FromQuery] bool? isActive = null,
        CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 1000);
        var query = db.ProductLines.AsNoTracking();
        if (organizationId.HasValue) query = query.Where(x => x.OrganizationId == organizationId.Value);
        if (!string.IsNullOrWhiteSpace(code)) query = query.Where(x => x.Code.Contains(code));
        if (!string.IsNullOrWhiteSpace(name)) query = query.Where(x => x.Name.Contains(name));
        if (isActive.HasValue) query = query.Where(x => x.IsActive == isActive.Value);
        var total = await query.CountAsync(ct);
        var rows = await query.OrderBy(x => x.Code)
            .Skip(skip).Take(take).Select(x => x.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<ProductLineDto>(rows, total, skip, take));
    }
}
