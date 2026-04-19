using System.Security.Claims;
using AWBlazorApp.Features.Identity.Domain; using AWBlazorApp.Features.Admin.Permissions.Domain;
using AWBlazorApp.Infrastructure.Persistence;
using AWBlazorApp.Shared.Dtos;
using AWBlazorApp.Features.Inventory.Domain;
using AWBlazorApp.Features.Inventory.Dtos;
using AWBlazorApp.Features.Inventory.Services;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.Inventory.Api;

public static class InventoryTransactionEndpoints
{
    public static IEndpointRouteBuilder MapInventoryTransactionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/inventory-transactions")
            .WithTags("InventoryTransactions")
            .RequireAuthorization("ApiOrCookie");

        group.MapGet("/", ListAsync).WithName("ListInventoryTransactions");
        group.MapGet("/{id:long}", GetAsync).WithName("GetInventoryTransaction");
        group.MapPost("/", PostAsync).WithName("PostInventoryTransaction")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));

        return app;
    }

    private static async Task<Ok<PagedResult<InventoryTransactionDto>>> ListAsync(
        ApplicationDbContext db,
        [FromQuery] int skip = 0, [FromQuery] int take = 100,
        [FromQuery] int? inventoryItemId = null, [FromQuery] int? transactionTypeId = null,
        [FromQuery] int? fromLocationId = null, [FromQuery] int? toLocationId = null,
        [FromQuery] int? lotId = null, [FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null,
        [FromQuery] TransactionReferenceKind? referenceType = null, [FromQuery] int? referenceId = null,
        CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 1000);
        var query = db.InventoryTransactions.AsNoTracking();
        if (inventoryItemId.HasValue) query = query.Where(x => x.InventoryItemId == inventoryItemId.Value);
        if (transactionTypeId.HasValue) query = query.Where(x => x.TransactionTypeId == transactionTypeId.Value);
        if (fromLocationId.HasValue) query = query.Where(x => x.FromLocationId == fromLocationId.Value);
        if (toLocationId.HasValue) query = query.Where(x => x.ToLocationId == toLocationId.Value);
        if (lotId.HasValue) query = query.Where(x => x.LotId == lotId.Value);
        if (from.HasValue) query = query.Where(x => x.OccurredAt >= from.Value);
        if (to.HasValue) query = query.Where(x => x.OccurredAt < to.Value);
        if (referenceType.HasValue) query = query.Where(x => x.ReferenceType == referenceType.Value);
        if (referenceId.HasValue) query = query.Where(x => x.ReferenceId == referenceId.Value);
        var total = await query.CountAsync(ct);
        var rows = await query.OrderByDescending(x => x.OccurredAt).ThenByDescending(x => x.Id)
            .Skip(skip).Take(take).Select(x => x.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<InventoryTransactionDto>(rows, total, skip, take));
    }

    private static async Task<Results<Ok<InventoryTransactionDto>, NotFound>> GetAsync(
        long id, ApplicationDbContext db, CancellationToken ct)
    {
        var row = await db.InventoryTransactions.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        return row is null ? TypedResults.NotFound() : TypedResults.Ok(row.ToDto());
    }

    private static async Task<Results<Created<PostTransactionResult>, ValidationProblem, BadRequest<string>>> PostAsync(
        PostInventoryTransactionRequest request,
        IValidator<PostInventoryTransactionRequest> validator,
        IInventoryService service, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());

        try
        {
            var result = await service.PostTransactionAsync(new PostTransactionRequest(
                request.TypeCode!,
                request.InventoryItemId,
                request.Quantity,
                request.UnitMeasureCode!,
                request.FromLocationId,
                request.ToLocationId,
                request.LotId,
                request.SerialUnitId,
                request.FromStatus,
                request.ToStatus,
                request.ReferenceType,
                request.ReferenceId,
                request.ReferenceLineId,
                request.Notes,
                request.CorrelationId,
                request.OccurredAt), user.Identity?.Name, ct);
            return TypedResults.Created($"/api/inventory-transactions/{result.TransactionId}", result);
        }
        catch (InvalidOperationException ex)
        {
            return TypedResults.BadRequest(ex.Message);
        }
    }
}
