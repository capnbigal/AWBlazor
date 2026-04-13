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

public static class ProductModelIllustrationEndpoints
{
    public static IEndpointRouteBuilder MapProductModelIllustrationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/aw/product-model-illustrations")
            .WithTags("ProductModelIllustrations")
            .RequireAuthorization("ApiOrCookie");

        group.MapGet("/", ListAsync).WithName("ListProductModelIllustrations")
            .WithSummary("List Production.ProductModelIllustration rows. Composite PK = (ProductModelID, IllustrationID).");
        group.MapGet("/by-key", GetAsync).WithName("GetProductModelIllustration");
        group.MapPost("/", CreateAsync).WithName("CreateProductModelIllustration")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));
        group.MapPatch("/by-key", UpdateAsync).WithName("UpdateProductModelIllustration")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));
        group.MapDelete("/by-key", DeleteAsync).WithName("DeleteProductModelIllustration")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));
        group.MapGet("/history", HistoryAsync).WithName("ListProductModelIllustrationHistory");
        return app;
    }

    private static async Task<Ok<PagedResult<ProductModelIllustrationDto>>> ListAsync(
        ApplicationDbContext db, [FromQuery] int skip = 0, [FromQuery] int take = 50,
        [FromQuery] int? productModelId = null, CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 1000);
        var query = db.ProductModelIllustrations.AsNoTracking();
        if (productModelId.HasValue) query = query.Where(x => x.ProductModelId == productModelId.Value);
        var total = await query.CountAsync(ct);
        var rows = await query.OrderBy(x => x.ProductModelId).ThenBy(x => x.IllustrationId)
            .Skip(skip).Take(take).Select(x => x.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<ProductModelIllustrationDto>(rows, total, skip, take));
    }

    private static async Task<Results<Ok<ProductModelIllustrationDto>, NotFound>> GetAsync(
        [FromQuery] int productModelId, [FromQuery] int illustrationId,
        ApplicationDbContext db, CancellationToken ct)
    {
        var row = await db.ProductModelIllustrations.AsNoTracking()
            .FirstOrDefaultAsync(x => x.ProductModelId == productModelId && x.IllustrationId == illustrationId, ct);
        return row is null ? TypedResults.NotFound() : TypedResults.Ok(row.ToDto());
    }

    private static async Task<Results<Created<CompositeKeyResponse>, Conflict<string>, ValidationProblem>> CreateAsync(
        CreateProductModelIllustrationRequest request,
        IValidator<CreateProductModelIllustrationRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());

        if (await db.ProductModelIllustrations.AnyAsync(x =>
                x.ProductModelId == request.ProductModelId && x.IllustrationId == request.IllustrationId, ct))
        {
            return TypedResults.Conflict($"Junction row ({request.ProductModelId}, {request.IllustrationId}) already exists.");
        }

        var entity = request.ToEntity();
        db.ProductModelIllustrations.Add(entity);
        await db.SaveChangesAsync(ct);
        db.ProductModelIllustrationAuditLogs.Add(
            ProductModelIllustrationAuditService.RecordCreate(entity, user.Identity?.Name));
        await db.SaveChangesAsync(ct);
        return TypedResults.Created(
            $"/api/aw/product-model-illustrations/by-key?productModelId={entity.ProductModelId}&illustrationId={entity.IllustrationId}",
            new CompositeKeyResponse(new Dictionary<string, object>
            {
                ["productModelId"] = entity.ProductModelId,
                ["illustrationId"] = entity.IllustrationId,
            }));
    }

    private static async Task<Results<Ok<CompositeKeyResponse>, NotFound, ValidationProblem>> UpdateAsync(
        [FromQuery] int productModelId, [FromQuery] int illustrationId,
        UpdateProductModelIllustrationRequest request,
        IValidator<UpdateProductModelIllustrationRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());

        var entity = await db.ProductModelIllustrations
            .FirstOrDefaultAsync(x => x.ProductModelId == productModelId && x.IllustrationId == illustrationId, ct);
        if (entity is null) return TypedResults.NotFound();

        request.ApplyTo(entity);
        db.ProductModelIllustrationAuditLogs.Add(
            ProductModelIllustrationAuditService.RecordUpdate(entity, user.Identity?.Name));
        await db.SaveChangesAsync(ct);
        return TypedResults.Ok(new CompositeKeyResponse(new Dictionary<string, object>
        {
            ["productModelId"] = entity.ProductModelId,
            ["illustrationId"] = entity.IllustrationId,
        }));
    }

    private static async Task<Results<NoContent, NotFound>> DeleteAsync(
        [FromQuery] int productModelId, [FromQuery] int illustrationId,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var entity = await db.ProductModelIllustrations
            .FirstOrDefaultAsync(x => x.ProductModelId == productModelId && x.IllustrationId == illustrationId, ct);
        if (entity is null) return TypedResults.NotFound();

        db.ProductModelIllustrationAuditLogs.Add(
            ProductModelIllustrationAuditService.RecordDelete(entity, user.Identity?.Name));
        db.ProductModelIllustrations.Remove(entity);
        await db.SaveChangesAsync(ct);
        return TypedResults.NoContent();
    }

    private static async Task<Ok<List<ProductModelIllustrationAuditLogDto>>> HistoryAsync(
        ApplicationDbContext db,
        [FromQuery] int? productModelId = null,
        [FromQuery] int? illustrationId = null,
        CancellationToken ct = default)
    {
        var query = db.ProductModelIllustrationAuditLogs.AsNoTracking();
        if (productModelId.HasValue) query = query.Where(a => a.ProductModelId == productModelId.Value);
        if (illustrationId.HasValue) query = query.Where(a => a.IllustrationId == illustrationId.Value);

        var rows = await query
            .OrderByDescending(a => a.ChangedDate).ThenByDescending(a => a.Id)
            .Select(a => a.ToDto())
            .ToListAsync(ct);
        return TypedResults.Ok(rows);
    }
}
