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

public static class SalesPersonEndpoints
{
    public static IEndpointRouteBuilder MapSalesPersonEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/aw/sales-persons")
            .WithTags("SalesPersons")
            .RequireAuthorization("ApiOrCookie");

        group.MapGet("/", ListAsync).WithName("ListSalesPersons").WithSummary("List Sales.SalesPerson rows.");

        group.MapIntIdCrud<SalesPerson, SalesPersonDto, CreateSalesPersonRequest, UpdateSalesPersonRequest, SalesPersonAuditLog, SalesPersonAuditLogDto, SalesPersonAuditService.Snapshot, int>(
            entityName: "SalesPerson",
            routePrefix: "/api/aw/sales-persons",
            entitySet: db => db.SalesPersons,
            auditSet: db => db.SalesPersonAuditLogs,
            idSelector: e => e.Id,
            auditIdSelector: a => a.SalesPersonId,
            auditChangedDateSelector: a => a.ChangedDate,
            auditPrimaryKeySelector: a => a.Id,
            getId: e => e.Id,
            toDto: e => e.ToDto(),
            toEntity: r => r.ToEntity(),
            applyUpdate: (r, e) => r.ApplyTo(e),
            captureSnapshot: SalesPersonAuditService.CaptureSnapshot,
            recordCreate: SalesPersonAuditService.RecordCreate,
            recordUpdate: SalesPersonAuditService.RecordUpdate,
            recordDelete: SalesPersonAuditService.RecordDelete,
            auditToDto: a => a.ToDto());

        return app;
    }

    private static async Task<Ok<PagedResult<SalesPersonDto>>> ListAsync(
        ApplicationDbContext db, [FromQuery] int skip = 0, [FromQuery] int take = 50,
        [FromQuery] int? territoryId = null, CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 1000);
        var query = db.SalesPersons.AsNoTracking();
        if (territoryId.HasValue) query = query.Where(x => x.TerritoryId == territoryId.Value);
        var total = await query.CountAsync(ct);
        var rows = await query.OrderBy(x => x.Id).Skip(skip).Take(take).Select(x => x.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<SalesPersonDto>(rows, total, skip, take));
    }
}