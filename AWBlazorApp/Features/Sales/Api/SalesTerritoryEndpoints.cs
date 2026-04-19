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

public static class SalesTerritoryEndpoints
{
    public static IEndpointRouteBuilder MapSalesTerritoryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/aw/sales-territories")
            .WithTags("SalesTerritories")
            .RequireAuthorization("ApiOrCookie");

        group.MapGet("/", ListAsync).WithName("ListSalesTerritories").WithSummary("List Sales.SalesTerritory rows.");

        group.MapIntIdCrud<SalesTerritory, SalesTerritoryDto, CreateSalesTerritoryRequest, UpdateSalesTerritoryRequest, SalesTerritoryAuditLog, SalesTerritoryAuditLogDto, SalesTerritoryAuditService.Snapshot, int>(
            entityName: "SalesTerritory",
            routePrefix: "/api/aw/sales-territories",
            entitySet: db => db.SalesTerritories,
            auditSet: db => db.SalesTerritoryAuditLogs,
            idSelector: e => e.Id,
            auditIdSelector: a => a.SalesTerritoryId,
            auditChangedDateSelector: a => a.ChangedDate,
            auditPrimaryKeySelector: a => a.Id,
            getId: e => e.Id,
            toDto: e => e.ToDto(),
            toEntity: r => r.ToEntity(),
            applyUpdate: (r, e) => r.ApplyTo(e),
            captureSnapshot: SalesTerritoryAuditService.CaptureSnapshot,
            recordCreate: SalesTerritoryAuditService.RecordCreate,
            recordUpdate: SalesTerritoryAuditService.RecordUpdate,
            recordDelete: SalesTerritoryAuditService.RecordDelete,
            auditToDto: a => a.ToDto());

        return app;
    }

    private static async Task<Ok<PagedResult<SalesTerritoryDto>>> ListAsync(
        ApplicationDbContext db, [FromQuery] int skip = 0, [FromQuery] int take = 50,
        [FromQuery] string? name = null, [FromQuery] string? groupName = null, CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 1000);
        var query = db.SalesTerritories.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(name)) query = query.Where(x => x.Name.Contains(name));
        if (!string.IsNullOrWhiteSpace(groupName)) query = query.Where(x => x.GroupName == groupName);
        var total = await query.CountAsync(ct);
        var rows = await query.OrderBy(x => x.Id).Skip(skip).Take(take).Select(x => x.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<SalesTerritoryDto>(rows, total, skip, take));
    }
}