using AWBlazorApp.Features.Identity.Domain; using AWBlazorApp.Features.Admin.Permissions.Domain;
using AWBlazorApp.Infrastructure.Persistence;
using AWBlazorApp.Shared.Api;
using AWBlazorApp.Shared.Dtos;
using AWBlazorApp.Features.Enterprise.Assets.Domain;
using AWBlazorApp.Features.Enterprise.Assets.Dtos;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.Enterprise.Assets.Api;

public static class AssetEndpoints
{
    public static IEndpointRouteBuilder MapAssetEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/assets")
            .WithTags("Assets")
            .RequireAuthorization("ApiOrCookie");

        group.MapGet("/", ListAsync).WithName("ListAssets").WithSummary("List org.Asset rows.");

        // Get / Create / Update / Delete / History — audit handled automatically by
        // AuditLogInterceptor via SaveChangesAsync.
        group.MapCrudWithInterceptor<Asset, AssetDto, CreateAssetRequest, UpdateAssetRequest, int>(
            entityName: "Asset",
            routePrefix: "/api/assets",
            entitySet: db => db.Assets,
            idSelector: e => e.Id,
            getId: e => e.Id,
            toDto: e => e.ToDto(),
            toEntity: r => r.ToEntity(),
            applyUpdate: (r, e) => r.ApplyTo(e));

        return app;
    }

    private static async Task<Ok<PagedResult<AssetDto>>> ListAsync(
        ApplicationDbContext db,
        [FromQuery] int skip = 0, [FromQuery] int take = 50,
        [FromQuery] int? organizationId = null, [FromQuery] int? orgUnitId = null,
        [FromQuery] string? assetTag = null, [FromQuery] string? name = null,
        [FromQuery] AssetType? assetType = null, [FromQuery] AssetStatus? status = null,
        CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 1000);
        var query = db.Assets.AsNoTracking();
        if (organizationId.HasValue) query = query.Where(x => x.OrganizationId == organizationId.Value);
        if (orgUnitId.HasValue) query = query.Where(x => x.OrgUnitId == orgUnitId.Value);
        if (!string.IsNullOrWhiteSpace(assetTag)) query = query.Where(x => x.AssetTag.Contains(assetTag));
        if (!string.IsNullOrWhiteSpace(name)) query = query.Where(x => x.Name.Contains(name));
        if (assetType.HasValue) query = query.Where(x => x.AssetType == assetType.Value);
        if (status.HasValue) query = query.Where(x => x.Status == status.Value);
        var total = await query.CountAsync(ct);
        var rows = await query.OrderBy(x => x.AssetTag)
            .Skip(skip).Take(take).Select(x => x.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<AssetDto>(rows, total, skip, take));
    }
}
