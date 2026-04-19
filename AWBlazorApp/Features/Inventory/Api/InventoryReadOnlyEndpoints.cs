using AWBlazorApp.Features.Identity.Domain; using AWBlazorApp.Features.Admin.Permissions.Domain;
using AWBlazorApp.Infrastructure.Persistence;
using AWBlazorApp.Shared.Dtos;
using AWBlazorApp.Features.Inventory.Domain;
using AWBlazorApp.Features.Inventory.Dtos;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.Inventory.Api;

/// <summary>
/// Read endpoints for derived / reference / operational inventory tables that aren't created
/// through a user-facing API: balances, transaction types, outbox, queue. The outbox and queue
/// do expose a retry/inspect write endpoint for the ops UI.
/// </summary>
public static class InventoryReadOnlyEndpoints
{
    public static IEndpointRouteBuilder MapInventoryReadOnlyEndpoints(this IEndpointRouteBuilder app)
    {
        MapBalances(app);
        MapTypes(app);
        MapOutbox(app);
        MapQueue(app);
        return app;
    }

    private static void MapBalances(IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/api/inventory-balances").WithTags("InventoryBalances")
            .RequireAuthorization("ApiOrCookie");
        g.MapGet("/", async (
            ApplicationDbContext db,
            [FromQuery] int skip = 0, [FromQuery] int take = 100,
            [FromQuery] int? inventoryItemId = null, [FromQuery] int? locationId = null,
            [FromQuery] int? lotId = null, [FromQuery] BalanceStatus? status = null,
            [FromQuery] bool nonZero = false,
            CancellationToken ct = default) =>
        {
            take = Math.Clamp(take, 1, 1000);
            var query = db.InventoryBalances.AsNoTracking();
            if (inventoryItemId.HasValue) query = query.Where(x => x.InventoryItemId == inventoryItemId.Value);
            if (locationId.HasValue) query = query.Where(x => x.LocationId == locationId.Value);
            if (lotId.HasValue) query = query.Where(x => x.LotId == lotId.Value);
            if (status.HasValue) query = query.Where(x => x.Status == status.Value);
            if (nonZero) query = query.Where(x => x.Quantity != 0);
            var total = await query.CountAsync(ct);
            var rows = await query.OrderBy(x => x.InventoryItemId).ThenBy(x => x.LocationId)
                .Skip(skip).Take(take).Select(x => x.ToDto()).ToListAsync(ct);
            return TypedResults.Ok(new PagedResult<InventoryBalanceDto>(rows, total, skip, take));
        }).WithName("ListInventoryBalances");
    }

    private static void MapTypes(IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/api/inventory-transaction-types").WithTags("InventoryTransactionTypes")
            .RequireAuthorization("ApiOrCookie");
        g.MapGet("/", async (ApplicationDbContext db, CancellationToken ct) =>
        {
            var rows = await db.InventoryTransactionTypes.AsNoTracking()
                .OrderBy(t => t.Id).Select(t => t.ToDto()).ToListAsync(ct);
            return TypedResults.Ok(new PagedResult<InventoryTransactionTypeDto>(rows, rows.Count, 0, rows.Count));
        }).WithName("ListInventoryTransactionTypes");
    }

    private static void MapOutbox(IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/api/inventory-outbox").WithTags("InventoryOutbox")
            .RequireAuthorization("ApiOrCookie");
        g.MapGet("/", async (
            ApplicationDbContext db,
            [FromQuery] int skip = 0, [FromQuery] int take = 50,
            [FromQuery] OutboxStatus? status = null,
            CancellationToken ct = default) =>
        {
            take = Math.Clamp(take, 1, 500);
            var query = db.InventoryTransactionOutbox.AsNoTracking();
            if (status.HasValue) query = query.Where(x => x.Status == status.Value);
            var total = await query.CountAsync(ct);
            var rows = await query.OrderByDescending(x => x.CreatedAt)
                .Skip(skip).Take(take).Select(x => x.ToDto()).ToListAsync(ct);
            return TypedResults.Ok(new PagedResult<InventoryOutboxDto>(rows, total, skip, take));
        }).WithName("ListInventoryOutbox");

        g.MapPost("/{id:long}/retry", async (long id, ApplicationDbContext db, CancellationToken ct) =>
        {
            var row = await db.InventoryTransactionOutbox.FirstOrDefaultAsync(x => x.Id == id, ct);
            if (row is null) return Results.NotFound();
            if (row.Status == OutboxStatus.Published) return Results.BadRequest("Already published.");
            row.Status = OutboxStatus.Pending;
            row.NextAttemptAt = DateTime.UtcNow;
            row.LastError = null;
            await db.SaveChangesAsync(ct);
            return Results.Ok(new IdResponse((int)id));
        }).WithName("RetryInventoryOutbox")
          .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));
    }

    private static void MapQueue(IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/api/inventory-queue").WithTags("InventoryQueue")
            .RequireAuthorization("ApiOrCookie");
        g.MapGet("/", async (
            ApplicationDbContext db,
            [FromQuery] int skip = 0, [FromQuery] int take = 50,
            [FromQuery] QueueParseStatus? parseStatus = null, [FromQuery] QueueProcessStatus? processStatus = null,
            [FromQuery] TransactionQueueSource? source = null,
            CancellationToken ct = default) =>
        {
            take = Math.Clamp(take, 1, 500);
            var query = db.InventoryTransactionQueue.AsNoTracking();
            if (parseStatus.HasValue) query = query.Where(x => x.ParseStatus == parseStatus.Value);
            if (processStatus.HasValue) query = query.Where(x => x.ProcessStatus == processStatus.Value);
            if (source.HasValue) query = query.Where(x => x.Source == source.Value);
            var total = await query.CountAsync(ct);
            var rows = await query.OrderByDescending(x => x.ReceivedAt)
                .Skip(skip).Take(take).Select(x => x.ToDto()).ToListAsync(ct);
            return TypedResults.Ok(new PagedResult<InventoryQueueDto>(rows, total, skip, take));
        }).WithName("ListInventoryQueue");
    }
}
