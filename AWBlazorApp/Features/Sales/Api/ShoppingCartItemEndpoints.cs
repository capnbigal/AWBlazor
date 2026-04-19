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

public static class ShoppingCartItemEndpoints
{
    public static IEndpointRouteBuilder MapShoppingCartItemEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/aw/shopping-cart-items")
            .WithTags("ShoppingCartItems")
            .RequireAuthorization("ApiOrCookie");

        group.MapGet("/", ListAsync).WithName("ListShoppingCartItems").WithSummary("List Sales.ShoppingCartItem rows.");

        group.MapIntIdCrud<ShoppingCartItem, ShoppingCartItemDto, CreateShoppingCartItemRequest, UpdateShoppingCartItemRequest, ShoppingCartItemAuditLog, ShoppingCartItemAuditLogDto, ShoppingCartItemAuditService.Snapshot, int>(
            entityName: "ShoppingCartItem",
            routePrefix: "/api/aw/shopping-cart-items",
            entitySet: db => db.ShoppingCartItems,
            auditSet: db => db.ShoppingCartItemAuditLogs,
            idSelector: e => e.Id,
            auditIdSelector: a => a.ShoppingCartItemId,
            auditChangedDateSelector: a => a.ChangedDate,
            auditPrimaryKeySelector: a => a.Id,
            getId: e => e.Id,
            toDto: e => e.ToDto(),
            toEntity: r => r.ToEntity(),
            applyUpdate: (r, e) => r.ApplyTo(e),
            captureSnapshot: ShoppingCartItemAuditService.CaptureSnapshot,
            recordCreate: ShoppingCartItemAuditService.RecordCreate,
            recordUpdate: ShoppingCartItemAuditService.RecordUpdate,
            recordDelete: ShoppingCartItemAuditService.RecordDelete,
            auditToDto: a => a.ToDto());

        return app;
    }

    private static async Task<Ok<PagedResult<ShoppingCartItemDto>>> ListAsync(
        ApplicationDbContext db, [FromQuery] int skip = 0, [FromQuery] int take = 50,
        [FromQuery] string? shoppingCartId = null, CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 1000);
        var query = db.ShoppingCartItems.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(shoppingCartId)) query = query.Where(x => x.ShoppingCartId == shoppingCartId);
        var total = await query.CountAsync(ct);
        var rows = await query.OrderBy(x => x.Id).Skip(skip).Take(take).Select(x => x.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<ShoppingCartItemDto>(rows, total, skip, take));
    }
}