using System.Security.Claims;
using AWBlazorApp.Data;
using AWBlazorApp.Models;
using AWBlazorApp.Models.AdventureWorks;
using AWBlazorApp.Services.AdventureWorksAudit;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Endpoints.AdventureWorks;

public static class ShoppingCartItemEndpoints
{
    public static IEndpointRouteBuilder MapShoppingCartItemEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/aw/shopping-cart-items")
            .WithTags("ShoppingCartItems")
            .RequireAuthorization("ApiOrCookie");

        group.MapGet("/", ListAsync).WithName("ListShoppingCartItems").WithSummary("List Sales.ShoppingCartItem rows.");
        group.MapGet("/{id:int}", GetAsync).WithName("GetShoppingCartItem");
        group.MapPost("/", CreateAsync).WithName("CreateShoppingCartItem")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));
        group.MapPatch("/{id:int}", UpdateAsync).WithName("UpdateShoppingCartItem")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));
        group.MapDelete("/{id:int}", DeleteAsync).WithName("DeleteShoppingCartItem")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));
        group.MapGet("/{id:int}/history", HistoryAsync).WithName("ListShoppingCartItemHistory");
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

    private static async Task<Results<Ok<ShoppingCartItemDto>, NotFound>> GetAsync(int id, ApplicationDbContext db, CancellationToken ct)
    {
        var row = await db.ShoppingCartItems.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        return row is null ? TypedResults.NotFound() : TypedResults.Ok(row.ToDto());
    }

    private static async Task<Results<Created<IdResponse>, ValidationProblem>> CreateAsync(
        CreateShoppingCartItemRequest request, IValidator<CreateShoppingCartItemRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());

        var entity = request.ToEntity();
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        db.ShoppingCartItems.Add(entity);
        await db.SaveChangesAsync(ct);
        db.ShoppingCartItemAuditLogs.Add(ShoppingCartItemAuditService.RecordCreate(entity, user.Identity?.Name));
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return TypedResults.Created($"/api/aw/shopping-cart-items/{entity.Id}", new IdResponse(entity.Id));
    }

    private static async Task<Results<Ok<IdResponse>, NotFound, ValidationProblem>> UpdateAsync(
        int id, UpdateShoppingCartItemRequest request, IValidator<UpdateShoppingCartItemRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());

        var entity = await db.ShoppingCartItems.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null) return TypedResults.NotFound();

        var before = ShoppingCartItemAuditService.CaptureSnapshot(entity);
        request.ApplyTo(entity);
        db.ShoppingCartItemAuditLogs.Add(ShoppingCartItemAuditService.RecordUpdate(before, entity, user.Identity?.Name));
        await db.SaveChangesAsync(ct);
        return TypedResults.Ok(new IdResponse(entity.Id));
    }

    private static async Task<Results<NoContent, NotFound>> DeleteAsync(
        int id, ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var entity = await db.ShoppingCartItems.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null) return TypedResults.NotFound();

        db.ShoppingCartItemAuditLogs.Add(ShoppingCartItemAuditService.RecordDelete(entity, user.Identity?.Name));
        db.ShoppingCartItems.Remove(entity);
        await db.SaveChangesAsync(ct);
        return TypedResults.NoContent();
    }

    private static async Task<Ok<List<ShoppingCartItemAuditLogDto>>> HistoryAsync(int id, ApplicationDbContext db, CancellationToken ct)
    {
        var rows = await db.ShoppingCartItemAuditLogs.AsNoTracking()
            .Where(a => a.ShoppingCartItemId == id)
            .OrderByDescending(a => a.ChangedDate).ThenByDescending(a => a.Id)
            .Select(a => a.ToDto())
            .ToListAsync(ct);
        return TypedResults.Ok(rows);
    }
}
