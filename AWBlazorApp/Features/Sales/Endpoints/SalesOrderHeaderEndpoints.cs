using AWBlazorApp.Data;
using AWBlazorApp.Infrastructure.Persistence;
using AWBlazorApp.Shared.Endpoints;
using AWBlazorApp.Shared.Models;
using AWBlazorApp.Features.Sales.Audit;
using AWBlazorApp.Features.Sales.Domain;
using AWBlazorApp.Features.Sales.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.Sales.Endpoints;

public static class SalesOrderHeaderEndpoints
{
    public static IEndpointRouteBuilder MapSalesOrderHeaderEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/aw/sales-order-headers")
            .WithTags("SalesOrderHeaders")
            .RequireAuthorization("ApiOrCookie");

        group.MapGet("/", ListAsync).WithName("ListSalesOrderHeaders").WithSummary("List Sales.SalesOrderHeader rows.");

        group.MapIntIdCrud<SalesOrderHeader, SalesOrderHeaderDto, CreateSalesOrderHeaderRequest, UpdateSalesOrderHeaderRequest, SalesOrderHeaderAuditLog, SalesOrderHeaderAuditLogDto, SalesOrderHeaderAuditService.Snapshot, int>(
            entityName: "SalesOrderHeader",
            routePrefix: "/api/aw/sales-order-headers",
            entitySet: db => db.SalesOrderHeaders,
            auditSet: db => db.SalesOrderHeaderAuditLogs,
            idSelector: e => e.Id,
            auditIdSelector: a => a.SalesOrderId,
            auditChangedDateSelector: a => a.ChangedDate,
            auditPrimaryKeySelector: a => a.Id,
            getId: e => e.Id,
            toDto: e => e.ToDto(),
            toEntity: r => r.ToEntity(),
            applyUpdate: (r, e) => r.ApplyTo(e),
            captureSnapshot: SalesOrderHeaderAuditService.CaptureSnapshot,
            recordCreate: SalesOrderHeaderAuditService.RecordCreate,
            recordUpdate: SalesOrderHeaderAuditService.RecordUpdate,
            recordDelete: SalesOrderHeaderAuditService.RecordDelete,
            auditToDto: a => a.ToDto());

        return app;
    }

    private static async Task<Ok<PagedResult<SalesOrderHeaderDto>>> ListAsync(
        ApplicationDbContext db, [FromQuery] int skip = 0, [FromQuery] int take = 50,
        [FromQuery] int? customerId = null, [FromQuery] byte? status = null,
        CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 1000);
        var query = db.SalesOrderHeaders.AsNoTracking();
        if (customerId.HasValue) query = query.Where(x => x.CustomerId == customerId.Value);
        if (status.HasValue) query = query.Where(x => x.Status == status.Value);
        var total = await query.CountAsync(ct);
        var rows = await query.OrderByDescending(x => x.OrderDate).ThenBy(x => x.Id)
            .Skip(skip).Take(take).Select(x => x.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<SalesOrderHeaderDto>(rows, total, skip, take));
    }
}