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

public static class StationEndpoints
{
    public static IEndpointRouteBuilder MapStationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/stations")
            .WithTags("Stations")
            .RequireAuthorization("ApiOrCookie");

        group.MapGet("/", ListAsync).WithName("ListStations").WithSummary("List org.Station rows.");

        group.MapIntIdCrud<Station, StationDto, CreateStationRequest, UpdateStationRequest, StationAuditLog, StationAuditLogDto, StationAuditService.Snapshot, int>(
            entityName: "Station",
            routePrefix: "/api/stations",
            entitySet: db => db.Stations,
            auditSet: db => db.StationAuditLogs,
            idSelector: e => e.Id,
            auditIdSelector: a => a.StationId,
            auditChangedDateSelector: a => a.ChangedDate,
            auditPrimaryKeySelector: a => a.Id,
            getId: e => e.Id,
            toDto: e => e.ToDto(),
            toEntity: r => r.ToEntity(),
            applyUpdate: (r, e) => r.ApplyTo(e),
            captureSnapshot: StationAuditService.CaptureSnapshot,
            recordCreate: StationAuditService.RecordCreate,
            recordUpdate: StationAuditService.RecordUpdate,
            recordDelete: StationAuditService.RecordDelete,
            auditToDto: a => a.ToDto());

        return app;
    }

    private static async Task<Ok<PagedResult<StationDto>>> ListAsync(
        ApplicationDbContext db,
        [FromQuery] int skip = 0, [FromQuery] int take = 50,
        [FromQuery] int? orgUnitId = null, [FromQuery] StationKind? stationKind = null,
        [FromQuery] int? operatorBusinessEntityId = null, [FromQuery] int? assetId = null,
        [FromQuery] bool? isActive = null,
        CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 1000);
        var query = db.Stations.AsNoTracking();
        if (orgUnitId.HasValue) query = query.Where(x => x.OrgUnitId == orgUnitId.Value);
        if (stationKind.HasValue) query = query.Where(x => x.StationKind == stationKind.Value);
        if (operatorBusinessEntityId.HasValue) query = query.Where(x => x.OperatorBusinessEntityId == operatorBusinessEntityId.Value);
        if (assetId.HasValue) query = query.Where(x => x.AssetId == assetId.Value);
        if (isActive.HasValue) query = query.Where(x => x.IsActive == isActive.Value);
        var total = await query.CountAsync(ct);
        var rows = await query.OrderBy(x => x.OrgUnitId).ThenBy(x => x.Code)
            .Skip(skip).Take(take).Select(x => x.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<StationDto>(rows, total, skip, take));
    }
}
