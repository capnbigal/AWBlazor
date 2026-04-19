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

public static class LocationEndpoints
{
    public static IEndpointRouteBuilder MapLocationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/aw/locations")
            .WithTags("Locations")
            .RequireAuthorization("ApiOrCookie");

        group.MapGet("/", ListAsync).WithName("ListLocations").WithSummary("List Production.Location rows.");

        group.MapIntIdCrud<Location, LocationDto, CreateLocationRequest, UpdateLocationRequest, LocationAuditLog, LocationAuditLogDto, LocationAuditService.Snapshot, short>(
            entityName: "Location",
            routePrefix: "/api/aw/locations",
            entitySet: db => db.Locations,
            auditSet: db => db.LocationAuditLogs,
            idSelector: e => e.Id,
            auditIdSelector: a => a.LocationId,
            auditChangedDateSelector: a => a.ChangedDate,
            auditPrimaryKeySelector: a => a.Id,
            getId: e => e.Id,
            toDto: e => e.ToDto(),
            toEntity: r => r.ToEntity(),
            applyUpdate: (r, e) => r.ApplyTo(e),
            captureSnapshot: LocationAuditService.CaptureSnapshot,
            recordCreate: LocationAuditService.RecordCreate,
            recordUpdate: LocationAuditService.RecordUpdate,
            recordDelete: LocationAuditService.RecordDelete,
            auditToDto: a => a.ToDto());

        return app;
    }

    private static async Task<Ok<PagedResult<LocationDto>>> ListAsync(
        ApplicationDbContext db, [FromQuery] int skip = 0, [FromQuery] int take = 50,
        [FromQuery] string? name = null, CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 1000);
        var query = db.Locations.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(name)) query = query.Where(x => x.Name.Contains(name));
        var total = await query.CountAsync(ct);
        var rows = await query.OrderBy(x => x.Id).Skip(skip).Take(take).Select(x => x.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<LocationDto>(rows, total, skip, take));
    }
}