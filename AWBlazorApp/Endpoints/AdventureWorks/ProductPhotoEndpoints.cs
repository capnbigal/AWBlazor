using System.Security.Claims;
using AWBlazorApp.Data;
using AWBlazorApp.Infrastructure.Persistence;
using AWBlazorApp.Models;
using AWBlazorApp.Models.AdventureWorks;
using AWBlazorApp.Services.AdventureWorksAudit;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Endpoints.AdventureWorks;

public static class ProductPhotoEndpoints
{
    public static IEndpointRouteBuilder MapProductPhotoEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/aw/product-photos")
            .WithTags("ProductPhotos")
            .RequireAuthorization("ApiOrCookie");

        group.MapGet("/", ListAsync).WithName("ListProductPhotos").WithSummary("List Production.ProductPhoto rows. Image bytes are not exposed.");
        group.MapGet("/{id:int}", GetAsync).WithName("GetProductPhoto");
        group.MapPost("/", CreateAsync).WithName("CreateProductPhoto")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));
        group.MapPatch("/{id:int}", UpdateAsync).WithName("UpdateProductPhoto")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));
        group.MapDelete("/{id:int}", DeleteAsync).WithName("DeleteProductPhoto")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));
        group.MapGet("/{id:int}/history", HistoryAsync).WithName("ListProductPhotoHistory");
        return app;
    }

    private static async Task<Ok<PagedResult<ProductPhotoDto>>> ListAsync(
        ApplicationDbContext db, [FromQuery] int skip = 0, [FromQuery] int take = 50,
        CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 1000);
        var query = db.ProductPhotos.AsNoTracking();
        var total = await query.CountAsync(ct);
        // Don't pull image bytes back for the list — project to DTO inside the SQL projection.
        var rows = await query.OrderBy(x => x.Id).Skip(skip).Take(take)
            .Select(x => new ProductPhotoDto(
                x.Id, x.ThumbnailPhotoFileName, x.LargePhotoFileName,
                x.ThumbNailPhoto != null && x.ThumbNailPhoto.Length > 0,
                x.LargePhoto != null && x.LargePhoto.Length > 0,
                x.ModifiedDate))
            .ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<ProductPhotoDto>(rows, total, skip, take));
    }

    private static async Task<Results<Ok<ProductPhotoDto>, NotFound>> GetAsync(int id, ApplicationDbContext db, CancellationToken ct)
    {
        var row = await db.ProductPhotos.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        return row is null ? TypedResults.NotFound() : TypedResults.Ok(row.ToDto());
    }

    private static async Task<Results<Created<IdResponse>, ValidationProblem>> CreateAsync(
        CreateProductPhotoRequest request, IValidator<CreateProductPhotoRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());

        var entity = request.ToEntity();
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        db.ProductPhotos.Add(entity);
        await db.SaveChangesAsync(ct);
        db.ProductPhotoAuditLogs.Add(ProductPhotoAuditService.RecordCreate(entity, user.Identity?.Name));
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return TypedResults.Created($"/api/aw/product-photos/{entity.Id}", new IdResponse(entity.Id));
    }

    private static async Task<Results<Ok<IdResponse>, NotFound, ValidationProblem>> UpdateAsync(
        int id, UpdateProductPhotoRequest request, IValidator<UpdateProductPhotoRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());

        var entity = await db.ProductPhotos.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null) return TypedResults.NotFound();

        var before = ProductPhotoAuditService.CaptureSnapshot(entity);
        request.ApplyTo(entity);
        db.ProductPhotoAuditLogs.Add(ProductPhotoAuditService.RecordUpdate(before, entity, user.Identity?.Name));
        await db.SaveChangesAsync(ct);
        return TypedResults.Ok(new IdResponse(entity.Id));
    }

    private static async Task<Results<NoContent, NotFound>> DeleteAsync(
        int id, ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var entity = await db.ProductPhotos.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null) return TypedResults.NotFound();

        db.ProductPhotoAuditLogs.Add(ProductPhotoAuditService.RecordDelete(entity, user.Identity?.Name));
        db.ProductPhotos.Remove(entity);
        await db.SaveChangesAsync(ct);
        return TypedResults.NoContent();
    }

    private static async Task<Ok<List<ProductPhotoAuditLogDto>>> HistoryAsync(int id, ApplicationDbContext db, CancellationToken ct)
    {
        var rows = await db.ProductPhotoAuditLogs.AsNoTracking()
            .Where(a => a.ProductPhotoId == id)
            .OrderByDescending(a => a.ChangedDate).ThenByDescending(a => a.Id)
            .Select(a => a.ToDto())
            .ToListAsync(ct);
        return TypedResults.Ok(rows);
    }
}
