using AWBlazorApp.Infrastructure.Persistence;
using AWBlazorApp.Shared.Api;
using AWBlazorApp.Shared.Dtos;
using AWBlazorApp.Features.Inventory.Lots.Domain;
using AWBlazorApp.Features.Inventory.Lots.Dtos;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.Inventory.Lots.Api;

public static class LotEndpoints
{
    public static IEndpointRouteBuilder MapLotEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/lots")
            .WithTags("Lots")
            .RequireAuthorization("ApiOrCookie");

        group.MapGet("/", ListAsync).WithName("ListLots");
        group.MapCrudWithInterceptor<Lot, LotDto, CreateLotRequest, UpdateLotRequest, int>(
            entityName: "Lot",
            routePrefix: "/api/lots",
            entitySet: db => db.Lots,
            idSelector: e => e.Id,
            getId: e => e.Id,
            toDto: e => e.ToDto(),
            toEntity: r => r.ToEntity(),
            applyUpdate: (r, e) => r.ApplyTo(e));
        return app;
    }

    private static async Task<Ok<PagedResult<LotDto>>> ListAsync(
        ApplicationDbContext db,
        [FromQuery] int skip = 0, [FromQuery] int take = 50,
        [FromQuery] int? inventoryItemId = null, [FromQuery] string? lotCode = null,
        [FromQuery] LotStatus? status = null,
        CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 1000);
        var query = db.Lots.AsNoTracking();
        if (inventoryItemId.HasValue) query = query.Where(x => x.InventoryItemId == inventoryItemId.Value);
        if (!string.IsNullOrWhiteSpace(lotCode)) query = query.Where(x => x.LotCode.Contains(lotCode));
        if (status.HasValue) query = query.Where(x => x.Status == status.Value);
        var total = await query.CountAsync(ct);
        var rows = await query.OrderByDescending(x => x.ReceivedAt ?? x.ManufacturedAt ?? DateTime.MinValue)
            .Skip(skip).Take(take).Select(x => x.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<LotDto>(rows, total, skip, take));
    }
}
