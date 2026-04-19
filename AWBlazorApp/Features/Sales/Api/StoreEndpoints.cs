using AWBlazorApp.Features.Identity.Domain; using AWBlazorApp.Features.Admin.Permissions.Domain;
using AWBlazorApp.Infrastructure.Persistence;
using AWBlazorApp.Shared.Api;
using AWBlazorApp.Shared.Dtos;
using AWBlazorApp.Features.Sales.Audit;
using AWBlazorApp.Features.Sales.Domain;
using AWBlazorApp.Features.Sales.Dtos;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.Sales.Api;

public static class StoreEndpoints
{
    public static IEndpointRouteBuilder MapStoreEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/aw/stores")
            .WithTags("Stores")
            .RequireAuthorization("ApiOrCookie");

        group.MapGet("/", ListAsync).WithName("ListStores").WithSummary("List Sales.Store rows.");

        group.MapIntIdCrud<Store, StoreDto, CreateStoreRequest, UpdateStoreRequest, StoreAuditLog, StoreAuditLogDto, StoreAuditService.Snapshot, int>(
            entityName: "Store",
            routePrefix: "/api/aw/stores",
            entitySet: db => db.Stores,
            auditSet: db => db.StoreAuditLogs,
            idSelector: e => e.Id,
            auditIdSelector: a => a.StoreId,
            auditChangedDateSelector: a => a.ChangedDate,
            auditPrimaryKeySelector: a => a.Id,
            getId: e => e.Id,
            toDto: e => e.ToDto(),
            toEntity: r => r.ToEntity(),
            applyUpdate: (r, e) => r.ApplyTo(e),
            captureSnapshot: StoreAuditService.CaptureSnapshot,
            recordCreate: StoreAuditService.RecordCreate,
            recordUpdate: StoreAuditService.RecordUpdate,
            recordDelete: StoreAuditService.RecordDelete,
            auditToDto: a => a.ToDto());

        return app;
    }

    private static async Task<Ok<PagedResult<StoreDto>>> ListAsync(
        ApplicationDbContext db, [FromQuery] int skip = 0, [FromQuery] int take = 50,
        [FromQuery] string? name = null, CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 1000);
        var query = db.Stores.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(name))
        {
            var n = name.Trim();
            query = query.Where(x => x.Name.Contains(n));
        }
        var total = await query.CountAsync(ct);
        var rows = await query.OrderBy(x => x.Name).Skip(skip).Take(take).Select(x => x.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<StoreDto>(rows, total, skip, take));
    }
}