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

public static class SpecialOfferProductEndpoints
{
    public static IEndpointRouteBuilder MapSpecialOfferProductEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/aw/special-offer-products")
            .WithTags("SpecialOfferProducts")
            .RequireAuthorization("ApiOrCookie");

        group.MapGet("/", ListAsync).WithName("ListSpecialOfferProducts")
            .WithSummary("List Sales.SpecialOfferProduct rows. Composite PK = (SpecialOfferID, ProductID).");
        group.MapGet("/by-key", GetAsync).WithName("GetSpecialOfferProduct");
        group.MapPost("/", CreateAsync).WithName("CreateSpecialOfferProduct")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));
        group.MapPatch("/by-key", UpdateAsync).WithName("UpdateSpecialOfferProduct")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));
        group.MapDelete("/by-key", DeleteAsync).WithName("DeleteSpecialOfferProduct")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));
        group.MapGet("/history", HistoryAsync).WithName("ListSpecialOfferProductHistory");
        return app;
    }

    private static async Task<Ok<PagedResult<SpecialOfferProductDto>>> ListAsync(
        ApplicationDbContext db, [FromQuery] int skip = 0, [FromQuery] int take = 50,
        [FromQuery] int? specialOfferId = null, [FromQuery] int? productId = null,
        CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 1000);
        var query = db.SpecialOfferProducts.AsNoTracking();
        if (specialOfferId.HasValue) query = query.Where(x => x.SpecialOfferId == specialOfferId.Value);
        if (productId.HasValue) query = query.Where(x => x.ProductId == productId.Value);
        var total = await query.CountAsync(ct);
        var rows = await query.OrderBy(x => x.SpecialOfferId).ThenBy(x => x.ProductId)
            .Skip(skip).Take(take).Select(x => x.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<SpecialOfferProductDto>(rows, total, skip, take));
    }

    private static async Task<Results<Ok<SpecialOfferProductDto>, NotFound>> GetAsync(
        [FromQuery] int specialOfferId, [FromQuery] int productId,
        ApplicationDbContext db, CancellationToken ct)
    {
        var row = await db.SpecialOfferProducts.AsNoTracking()
            .FirstOrDefaultAsync(x => x.SpecialOfferId == specialOfferId && x.ProductId == productId, ct);
        return row is null ? TypedResults.NotFound() : TypedResults.Ok(row.ToDto());
    }

    private static async Task<Results<Created<CompositeKeyResponse>, Conflict<string>, ValidationProblem>> CreateAsync(
        CreateSpecialOfferProductRequest request, IValidator<CreateSpecialOfferProductRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());

        if (await db.SpecialOfferProducts.AnyAsync(x =>
                x.SpecialOfferId == request.SpecialOfferId && x.ProductId == request.ProductId, ct))
        {
            return TypedResults.Conflict($"Junction row ({request.SpecialOfferId}, {request.ProductId}) already exists.");
        }

        var entity = request.ToEntity();
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        db.SpecialOfferProducts.Add(entity);
        await db.SaveChangesAsync(ct);
        db.SpecialOfferProductAuditLogs.Add(SpecialOfferProductAuditService.RecordCreate(entity, user.Identity?.Name));
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return TypedResults.Created(
            $"/api/aw/special-offer-products/by-key?specialOfferId={entity.SpecialOfferId}&productId={entity.ProductId}",
            new CompositeKeyResponse(new Dictionary<string, object>
            {
                ["specialOfferId"] = entity.SpecialOfferId,
                ["productId"] = entity.ProductId,
            }));
    }

    private static async Task<Results<Ok<CompositeKeyResponse>, NotFound, ValidationProblem>> UpdateAsync(
        [FromQuery] int specialOfferId, [FromQuery] int productId,
        UpdateSpecialOfferProductRequest request, IValidator<UpdateSpecialOfferProductRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());

        var entity = await db.SpecialOfferProducts
            .FirstOrDefaultAsync(x => x.SpecialOfferId == specialOfferId && x.ProductId == productId, ct);
        if (entity is null) return TypedResults.NotFound();

        request.ApplyTo(entity);
        db.SpecialOfferProductAuditLogs.Add(SpecialOfferProductAuditService.RecordUpdate(entity, user.Identity?.Name));
        await db.SaveChangesAsync(ct);
        return TypedResults.Ok(new CompositeKeyResponse(new Dictionary<string, object>
        {
            ["specialOfferId"] = entity.SpecialOfferId,
            ["productId"] = entity.ProductId,
        }));
    }

    private static async Task<Results<NoContent, NotFound>> DeleteAsync(
        [FromQuery] int specialOfferId, [FromQuery] int productId,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var entity = await db.SpecialOfferProducts
            .FirstOrDefaultAsync(x => x.SpecialOfferId == specialOfferId && x.ProductId == productId, ct);
        if (entity is null) return TypedResults.NotFound();

        db.SpecialOfferProductAuditLogs.Add(SpecialOfferProductAuditService.RecordDelete(entity, user.Identity?.Name));
        db.SpecialOfferProducts.Remove(entity);
        await db.SaveChangesAsync(ct);
        return TypedResults.NoContent();
    }

    private static async Task<Ok<List<SpecialOfferProductAuditLogDto>>> HistoryAsync(
        ApplicationDbContext db,
        [FromQuery] int? specialOfferId = null,
        [FromQuery] int? productId = null,
        CancellationToken ct = default)
    {
        var query = db.SpecialOfferProductAuditLogs.AsNoTracking();
        if (specialOfferId.HasValue) query = query.Where(a => a.SpecialOfferId == specialOfferId.Value);
        if (productId.HasValue) query = query.Where(a => a.ProductId == productId.Value);

        var rows = await query
            .OrderByDescending(a => a.ChangedDate).ThenByDescending(a => a.Id)
            .Select(a => a.ToDto())
            .ToListAsync(ct);
        return TypedResults.Ok(rows);
    }
}
