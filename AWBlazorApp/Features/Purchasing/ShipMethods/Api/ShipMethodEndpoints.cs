using AWBlazorApp.Features.Identity.Domain; using AWBlazorApp.Features.Admin.Permissions.Domain;
using AWBlazorApp.Infrastructure.Persistence;
using AWBlazorApp.Shared.Api;
using AWBlazorApp.Shared.Dtos;
using AWBlazorApp.Features.Purchasing.ProductVendors.Application.Services; using AWBlazorApp.Features.Purchasing.PurchaseOrderDetails.Application.Services; using AWBlazorApp.Features.Purchasing.PurchaseOrderHeaders.Application.Services; using AWBlazorApp.Features.Purchasing.ShipMethods.Application.Services; using AWBlazorApp.Features.Purchasing.Vendors.Application.Services; 
using AWBlazorApp.Features.Purchasing.ProductVendors.Domain; using AWBlazorApp.Features.Purchasing.PurchaseOrderDetails.Domain; using AWBlazorApp.Features.Purchasing.PurchaseOrderHeaders.Domain; using AWBlazorApp.Features.Purchasing.ShipMethods.Domain; using AWBlazorApp.Features.Purchasing.Vendors.Domain; 
using AWBlazorApp.Features.Purchasing.ProductVendors.Dtos; using AWBlazorApp.Features.Purchasing.PurchaseOrderDetails.Dtos; using AWBlazorApp.Features.Purchasing.PurchaseOrderHeaders.Dtos; using AWBlazorApp.Features.Purchasing.ShipMethods.Dtos; using AWBlazorApp.Features.Purchasing.Vendors.Dtos; 
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.Purchasing.ShipMethods.Api;

public static class ShipMethodEndpoints
{
    public static IEndpointRouteBuilder MapShipMethodEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/aw/ship-methods")
            .WithTags("ShipMethods")
            .RequireAuthorization("ApiOrCookie");

        group.MapGet("/", ListAsync).WithName("ListShipMethods").WithSummary("List Purchasing.ShipMethod rows.");

        group.MapIntIdCrud<ShipMethod, ShipMethodDto, CreateShipMethodRequest, UpdateShipMethodRequest, ShipMethodAuditLog, ShipMethodAuditLogDto, ShipMethodAuditService.Snapshot, int>(
            entityName: "ShipMethod",
            routePrefix: "/api/aw/ship-methods",
            entitySet: db => db.ShipMethods,
            auditSet: db => db.ShipMethodAuditLogs,
            idSelector: e => e.Id,
            auditIdSelector: a => a.ShipMethodId,
            auditChangedDateSelector: a => a.ChangedDate,
            auditPrimaryKeySelector: a => a.Id,
            getId: e => e.Id,
            toDto: e => e.ToDto(),
            toEntity: r => r.ToEntity(),
            applyUpdate: (r, e) => r.ApplyTo(e),
            captureSnapshot: ShipMethodAuditService.CaptureSnapshot,
            recordCreate: ShipMethodAuditService.RecordCreate,
            recordUpdate: ShipMethodAuditService.RecordUpdate,
            recordDelete: ShipMethodAuditService.RecordDelete,
            auditToDto: a => a.ToDto());

        return app;
    }

    private static async Task<Ok<PagedResult<ShipMethodDto>>> ListAsync(
        ApplicationDbContext db, [FromQuery] int skip = 0, [FromQuery] int take = 50,
        [FromQuery] string? name = null, CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 1000);
        var query = db.ShipMethods.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(name)) query = query.Where(x => x.Name.Contains(name));
        var total = await query.CountAsync(ct);
        var rows = await query.OrderBy(x => x.Id).Skip(skip).Take(take).Select(x => x.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<ShipMethodDto>(rows, total, skip, take));
    }
}