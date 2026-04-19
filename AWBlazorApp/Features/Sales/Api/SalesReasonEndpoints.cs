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

public static class SalesReasonEndpoints
{
    public static IEndpointRouteBuilder MapSalesReasonEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/aw/sales-reasons")
            .WithTags("SalesReasons")
            .RequireAuthorization("ApiOrCookie");

        group.MapGet("/", ListAsync).WithName("ListSalesReasons").WithSummary("List Sales.SalesReason rows.");

        group.MapIntIdCrud<SalesReason, SalesReasonDto, CreateSalesReasonRequest, UpdateSalesReasonRequest, SalesReasonAuditLog, SalesReasonAuditLogDto, SalesReasonAuditService.Snapshot, int>(
            entityName: "SalesReason",
            routePrefix: "/api/aw/sales-reasons",
            entitySet: db => db.SalesReasons,
            auditSet: db => db.SalesReasonAuditLogs,
            idSelector: e => e.Id,
            auditIdSelector: a => a.SalesReasonId,
            auditChangedDateSelector: a => a.ChangedDate,
            auditPrimaryKeySelector: a => a.Id,
            getId: e => e.Id,
            toDto: e => e.ToDto(),
            toEntity: r => r.ToEntity(),
            applyUpdate: (r, e) => r.ApplyTo(e),
            captureSnapshot: SalesReasonAuditService.CaptureSnapshot,
            recordCreate: SalesReasonAuditService.RecordCreate,
            recordUpdate: SalesReasonAuditService.RecordUpdate,
            recordDelete: SalesReasonAuditService.RecordDelete,
            auditToDto: a => a.ToDto());

        return app;
    }

    private static async Task<Ok<PagedResult<SalesReasonDto>>> ListAsync(
        ApplicationDbContext db, [FromQuery] int skip = 0, [FromQuery] int take = 50,
        [FromQuery] string? name = null, [FromQuery] string? reasonType = null, CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 1000);
        var query = db.SalesReasons.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(name)) query = query.Where(x => x.Name.Contains(name));
        if (!string.IsNullOrWhiteSpace(reasonType)) query = query.Where(x => x.ReasonType == reasonType);
        var total = await query.CountAsync(ct);
        var rows = await query.OrderBy(x => x.Id).Skip(skip).Take(take).Select(x => x.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<SalesReasonDto>(rows, total, skip, take));
    }
}