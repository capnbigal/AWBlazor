using AWBlazorApp.Features.Identity.Domain; using AWBlazorApp.Features.Admin.Permissions.Domain;
using AWBlazorApp.Infrastructure.Persistence;
using AWBlazorApp.Shared.Api;
using AWBlazorApp.Shared.Dtos;
using AWBlazorApp.Features.Purchasing.Audit;
using AWBlazorApp.Features.Purchasing.Domain;
using AWBlazorApp.Features.Purchasing.Dtos;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.Purchasing.Api;

public static class PurchaseOrderHeaderEndpoints
{
    public static IEndpointRouteBuilder MapPurchaseOrderHeaderEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/aw/purchase-order-headers")
            .WithTags("PurchaseOrderHeaders")
            .RequireAuthorization("ApiOrCookie");

        group.MapGet("/", ListAsync).WithName("ListPurchaseOrderHeaders").WithSummary("List Purchasing.PurchaseOrderHeader rows.");

        group.MapIntIdCrud<PurchaseOrderHeader, PurchaseOrderHeaderDto, CreatePurchaseOrderHeaderRequest, UpdatePurchaseOrderHeaderRequest, PurchaseOrderHeaderAuditLog, PurchaseOrderHeaderAuditLogDto, PurchaseOrderHeaderAuditService.Snapshot, int>(
            entityName: "PurchaseOrderHeader",
            routePrefix: "/api/aw/purchase-order-headers",
            entitySet: db => db.PurchaseOrderHeaders,
            auditSet: db => db.PurchaseOrderHeaderAuditLogs,
            idSelector: e => e.Id,
            auditIdSelector: a => a.PurchaseOrderId,
            auditChangedDateSelector: a => a.ChangedDate,
            auditPrimaryKeySelector: a => a.Id,
            getId: e => e.Id,
            toDto: e => e.ToDto(),
            toEntity: r => r.ToEntity(),
            applyUpdate: (r, e) => r.ApplyTo(e),
            captureSnapshot: PurchaseOrderHeaderAuditService.CaptureSnapshot,
            recordCreate: PurchaseOrderHeaderAuditService.RecordCreate,
            recordUpdate: PurchaseOrderHeaderAuditService.RecordUpdate,
            recordDelete: PurchaseOrderHeaderAuditService.RecordDelete,
            auditToDto: a => a.ToDto());

        return app;
    }

    private static async Task<Ok<PagedResult<PurchaseOrderHeaderDto>>> ListAsync(
        ApplicationDbContext db, [FromQuery] int skip = 0, [FromQuery] int take = 50,
        [FromQuery] int? vendorId = null, [FromQuery] byte? status = null, CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 1000);
        var query = db.PurchaseOrderHeaders.AsNoTracking();
        if (vendorId.HasValue) query = query.Where(x => x.VendorId == vendorId.Value);
        if (status.HasValue) query = query.Where(x => x.Status == status.Value);
        var total = await query.CountAsync(ct);
        var rows = await query.OrderByDescending(x => x.OrderDate).ThenBy(x => x.Id)
            .Skip(skip).Take(take).Select(x => x.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<PurchaseOrderHeaderDto>(rows, total, skip, take));
    }
}