using AWBlazorApp.Features.Identity.Domain; using AWBlazorApp.Features.Admin.Permissions.Domain;
using AWBlazorApp.Infrastructure.Persistence;
using AWBlazorApp.Shared.Api;
using AWBlazorApp.Shared.Dtos;
using AWBlazorApp.Features.Enterprise.Audit;
using AWBlazorApp.Features.Enterprise.Domain;
using AWBlazorApp.Features.Enterprise.Dtos;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.Enterprise.Api;

public static class AssetEndpoints
{
    public static IEndpointRouteBuilder MapAssetEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/assets")
            .WithTags("Assets")
            .RequireAuthorization("ApiOrCookie");

        group.MapGet("/", ListAsync).WithName("ListAssets").WithSummary("List org.Asset rows.");

        group.MapIntIdCrud<Asset, AssetDto, CreateAssetRequest, UpdateAssetRequest, AssetAuditLog, AssetAuditLogDto, AssetAuditService.Snapshot, int>(
            entityName: "Asset",
            routePrefix: "/api/assets",
            entitySet: db => db.Assets,
            auditSet: db => db.AssetAuditLogs,
            idSelector: e => e.Id,
            auditIdSelector: a => a.AssetId,
            auditChangedDateSelector: a => a.ChangedDate,
            auditPrimaryKeySelector: a => a.Id,
            getId: e => e.Id,
            toDto: e => e.ToDto(),
            toEntity: r => r.ToEntity(),
            applyUpdate: (r, e) => r.ApplyTo(e),
            captureSnapshot: AssetAuditService.CaptureSnapshot,
            recordCreate: AssetAuditService.RecordCreate,
            recordUpdate: AssetAuditService.RecordUpdate,
            recordDelete: AssetAuditService.RecordDelete,
            auditToDto: a => a.ToDto());

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
