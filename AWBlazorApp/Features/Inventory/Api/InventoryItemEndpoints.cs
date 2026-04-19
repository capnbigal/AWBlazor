using AWBlazorApp.Infrastructure.Persistence;
using AWBlazorApp.Shared.Api;
using AWBlazorApp.Shared.Dtos;
using AWBlazorApp.Features.Inventory.Audit;
using AWBlazorApp.Features.Inventory.Domain;
using AWBlazorApp.Features.Inventory.Dtos;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.Inventory.Api;

public static class InventoryItemEndpoints
{
    public static IEndpointRouteBuilder MapInventoryItemEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/inventory-items")
            .WithTags("InventoryItems")
            .RequireAuthorization("ApiOrCookie");

        group.MapGet("/", ListAsync).WithName("ListInventoryItems");
        group.MapIntIdCrud<InventoryItem, InventoryItemDto, CreateInventoryItemRequest, UpdateInventoryItemRequest, InventoryItemAuditLog, InventoryItemAuditLogDto, InventoryItemAuditService.Snapshot, int>(
            entityName: "InventoryItem",
            routePrefix: "/api/inventory-items",
            entitySet: db => db.InventoryItems,
            auditSet: db => db.InventoryItemAuditLogs,
            idSelector: e => e.Id,
            auditIdSelector: a => a.InventoryItemId,
            auditChangedDateSelector: a => a.ChangedDate,
            auditPrimaryKeySelector: a => a.Id,
            getId: e => e.Id,
            toDto: e => e.ToDto(),
            toEntity: r => r.ToEntity(),
            applyUpdate: (r, e) => r.ApplyTo(e),
            captureSnapshot: InventoryItemAuditService.CaptureSnapshot,
            recordCreate: InventoryItemAuditService.RecordCreate,
            recordUpdate: InventoryItemAuditService.RecordUpdate,
            recordDelete: InventoryItemAuditService.RecordDelete,
            auditToDto: a => a.ToDto());
        return app;
    }

    private static async Task<Ok<PagedResult<InventoryItemDto>>> ListAsync(
        ApplicationDbContext db,
        [FromQuery] int skip = 0, [FromQuery] int take = 50,
        [FromQuery] int? productId = null, [FromQuery] bool? tracksLot = null,
        [FromQuery] bool? tracksSerial = null, [FromQuery] bool? isActive = null,
        CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 1000);
        var query = db.InventoryItems.AsNoTracking();
        if (productId.HasValue) query = query.Where(x => x.ProductId == productId.Value);
        if (tracksLot.HasValue) query = query.Where(x => x.TracksLot == tracksLot.Value);
        if (tracksSerial.HasValue) query = query.Where(x => x.TracksSerial == tracksSerial.Value);
        if (isActive.HasValue) query = query.Where(x => x.IsActive == isActive.Value);
        var total = await query.CountAsync(ct);
        var rows = await query.OrderBy(x => x.ProductId).Skip(skip).Take(take).Select(x => x.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<InventoryItemDto>(rows, total, skip, take));
    }
}
