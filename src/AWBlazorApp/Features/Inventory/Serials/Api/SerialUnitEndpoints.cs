using AWBlazorApp.Infrastructure.Persistence;
using AWBlazorApp.Shared.Api;
using AWBlazorApp.Shared.Dtos;
using AWBlazorApp.Features.Inventory.Adjustments.Application.Services; using AWBlazorApp.Features.Inventory.Items.Application.Services; using AWBlazorApp.Features.Inventory.Locations.Application.Services; using AWBlazorApp.Features.Inventory.Lots.Application.Services; using AWBlazorApp.Features.Inventory.Serials.Application.Services; 
using AWBlazorApp.Features.Inventory.Adjustments.Domain; using AWBlazorApp.Features.Inventory.Items.Domain; using AWBlazorApp.Features.Inventory.Locations.Domain; using AWBlazorApp.Features.Inventory.Lots.Domain; using AWBlazorApp.Features.Inventory.Outbox.Domain; using AWBlazorApp.Features.Inventory.Queue.Domain; using AWBlazorApp.Features.Inventory.Reports.Domain; using AWBlazorApp.Features.Inventory.Serials.Domain; using AWBlazorApp.Features.Inventory.Transactions.Domain; using AWBlazorApp.Features.Inventory.Types.Domain; 
using AWBlazorApp.Features.Inventory.Adjustments.Dtos; using AWBlazorApp.Features.Inventory.Items.Dtos; using AWBlazorApp.Features.Inventory.Locations.Dtos; using AWBlazorApp.Features.Inventory.Lots.Dtos; using AWBlazorApp.Features.Inventory.Serials.Dtos; 
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.Inventory.Serials.Api;

public static class SerialUnitEndpoints
{
    public static IEndpointRouteBuilder MapSerialUnitEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/serial-units")
            .WithTags("SerialUnits")
            .RequireAuthorization("ApiOrCookie");

        group.MapGet("/", ListAsync).WithName("ListSerialUnits");
        group.MapIntIdCrud<SerialUnit, SerialUnitDto, CreateSerialUnitRequest, UpdateSerialUnitRequest, SerialUnitAuditLog, SerialUnitAuditLogDto, SerialUnitAuditService.Snapshot, int>(
            entityName: "SerialUnit",
            routePrefix: "/api/serial-units",
            entitySet: db => db.SerialUnits,
            auditSet: db => db.SerialUnitAuditLogs,
            idSelector: e => e.Id,
            auditIdSelector: a => a.SerialUnitId,
            auditChangedDateSelector: a => a.ChangedDate,
            auditPrimaryKeySelector: a => a.Id,
            getId: e => e.Id,
            toDto: e => e.ToDto(),
            toEntity: r => r.ToEntity(),
            applyUpdate: (r, e) => r.ApplyTo(e),
            captureSnapshot: SerialUnitAuditService.CaptureSnapshot,
            recordCreate: SerialUnitAuditService.RecordCreate,
            recordUpdate: SerialUnitAuditService.RecordUpdate,
            recordDelete: SerialUnitAuditService.RecordDelete,
            auditToDto: a => a.ToDto());
        return app;
    }

    private static async Task<Ok<PagedResult<SerialUnitDto>>> ListAsync(
        ApplicationDbContext db,
        [FromQuery] int skip = 0, [FromQuery] int take = 50,
        [FromQuery] int? inventoryItemId = null, [FromQuery] int? lotId = null,
        [FromQuery] string? serialNumber = null, [FromQuery] SerialUnitStatus? status = null,
        [FromQuery] int? currentLocationId = null,
        CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 1000);
        var query = db.SerialUnits.AsNoTracking();
        if (inventoryItemId.HasValue) query = query.Where(x => x.InventoryItemId == inventoryItemId.Value);
        if (lotId.HasValue) query = query.Where(x => x.LotId == lotId.Value);
        if (!string.IsNullOrWhiteSpace(serialNumber)) query = query.Where(x => x.SerialNumber.Contains(serialNumber));
        if (status.HasValue) query = query.Where(x => x.Status == status.Value);
        if (currentLocationId.HasValue) query = query.Where(x => x.CurrentLocationId == currentLocationId.Value);
        var total = await query.CountAsync(ct);
        var rows = await query.OrderBy(x => x.InventoryItemId).ThenBy(x => x.SerialNumber)
            .Skip(skip).Take(take).Select(x => x.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<SerialUnitDto>(rows, total, skip, take));
    }
}
