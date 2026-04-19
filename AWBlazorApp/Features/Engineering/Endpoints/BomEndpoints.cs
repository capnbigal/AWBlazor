using System.Security.Claims;
using AWBlazorApp.Data;
using AWBlazorApp.Features.Engineering.Audit;
using AWBlazorApp.Features.Engineering.Models;
using AWBlazorApp.Infrastructure.Persistence;
using AWBlazorApp.Shared.Models;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.Engineering.Endpoints;

public static class BomEndpoints
{
    public static IEndpointRouteBuilder MapBomEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/boms")
            .WithTags("Boms")
            .RequireAuthorization("ApiOrCookie");

        group.MapGet("/", ListAsync).WithName("ListBoms");
        group.MapGet("/{id:int}", GetAsync).WithName("GetBom");
        group.MapPost("/", CreateAsync).WithName("CreateBom")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));
        group.MapPatch("/{id:int}", UpdateAsync).WithName("UpdateBom")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));
        group.MapDelete("/{id:int}", DeleteAsync).WithName("DeleteBom")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Admin));
        group.MapGet("/{id:int}/history", HistoryAsync).WithName("ListBomHistory");

        group.MapGet("/{id:int}/lines", ListLinesAsync).WithName("ListBomLines");
        group.MapPost("/{id:int}/lines", CreateLineAsync).WithName("CreateBomLine")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));
        group.MapPatch("/lines/{lId:int}", UpdateLineAsync).WithName("UpdateBomLine")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));
        group.MapDelete("/lines/{lId:int}", DeleteLineAsync).WithName("DeleteBomLine")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));

        return app;
    }

    private static async Task<Ok<PagedResult<BomHeaderDto>>> ListAsync(
        ApplicationDbContext db,
        [FromQuery] int skip = 0, [FromQuery] int take = 50,
        [FromQuery] int? productId = null, [FromQuery] bool? isActive = null,
        CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 500);
        var q = db.BomHeaders.AsNoTracking();
        if (productId.HasValue) q = q.Where(x => x.ProductId == productId.Value);
        if (isActive.HasValue) q = q.Where(x => x.IsActive == isActive.Value);
        var total = await q.CountAsync(ct);
        var rows = await q.OrderBy(x => x.Code)
            .Skip(skip).Take(take).Select(x => x.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<BomHeaderDto>(rows, total, skip, take));
    }

    private static async Task<Results<Ok<BomHeaderDto>, NotFound>> GetAsync(int id, ApplicationDbContext db, CancellationToken ct)
    {
        var row = await db.BomHeaders.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        return row is null ? TypedResults.NotFound() : TypedResults.Ok(row.ToDto());
    }

    private static async Task<Results<Created<IdResponse>, ValidationProblem>> CreateAsync(
        CreateBomHeaderRequest request,
        IValidator<CreateBomHeaderRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());
        var entity = request.ToEntity();
        await db.AddWithAuditAsync(entity, e => BomHeaderAuditService.RecordCreate(e, user.Identity?.Name), ct);
        return TypedResults.Created($"/api/boms/{entity.Id}", new IdResponse(entity.Id));
    }

    private static async Task<Results<Ok<IdResponse>, NotFound, ValidationProblem>> UpdateAsync(
        int id, UpdateBomHeaderRequest request,
        IValidator<UpdateBomHeaderRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());
        var entity = await db.BomHeaders.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null) return TypedResults.NotFound();
        var before = BomHeaderAuditService.CaptureSnapshot(entity);
        request.ApplyTo(entity);
        db.BomHeaderAuditLogs.Add(BomHeaderAuditService.RecordUpdate(before, entity, user.Identity?.Name));
        await db.SaveChangesAsync(ct);
        return TypedResults.Ok(new IdResponse(entity.Id));
    }

    private static async Task<Results<NoContent, NotFound>> DeleteAsync(
        int id, ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var entity = await db.BomHeaders.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null) return TypedResults.NotFound();
        await db.DeleteWithAuditAsync(entity, BomHeaderAuditService.RecordDelete(entity, user.Identity?.Name), ct);
        return TypedResults.NoContent();
    }

    private static async Task<Ok<PagedResult<BomHeaderAuditLogDto>>> HistoryAsync(
        int id, ApplicationDbContext db,
        [FromQuery] int skip = 0, [FromQuery] int take = 50, CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 500);
        var q = db.BomHeaderAuditLogs.AsNoTracking().Where(a => a.BomHeaderId == id);
        var total = await q.CountAsync(ct);
        var rows = await q.OrderByDescending(a => a.ChangedDate).ThenByDescending(a => a.Id)
            .Skip(skip).Take(take).Select(a => a.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<BomHeaderAuditLogDto>(rows, total, skip, take));
    }

    private static async Task<Ok<PagedResult<BomLineDto>>> ListLinesAsync(
        int id, ApplicationDbContext db, CancellationToken ct = default)
    {
        var rows = await db.BomLines.AsNoTracking()
            .Where(l => l.BomHeaderId == id)
            .OrderBy(l => l.ComponentProductId)
            .Select(l => l.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<BomLineDto>(rows, rows.Count, 0, rows.Count));
    }

    private static async Task<Results<Created<IdResponse>, NotFound, ValidationProblem>> CreateLineAsync(
        int id, CreateBomLineRequest request,
        IValidator<CreateBomLineRequest> validator,
        ApplicationDbContext db, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());
        var parent = await db.BomHeaders.AsNoTracking().FirstOrDefaultAsync(b => b.Id == id, ct);
        if (parent is null) return TypedResults.NotFound();
        request = request with { BomHeaderId = id };
        var entity = request.ToEntity();
        db.BomLines.Add(entity);
        await db.SaveChangesAsync(ct);
        return TypedResults.Created($"/api/boms/{id}/lines/{entity.Id}", new IdResponse(entity.Id));
    }

    private static async Task<Results<Ok<IdResponse>, NotFound>> UpdateLineAsync(
        int lId, UpdateBomLineRequest request, ApplicationDbContext db, CancellationToken ct)
    {
        var entity = await db.BomLines.FirstOrDefaultAsync(l => l.Id == lId, ct);
        if (entity is null) return TypedResults.NotFound();
        request.ApplyTo(entity);
        await db.SaveChangesAsync(ct);
        return TypedResults.Ok(new IdResponse(entity.Id));
    }

    private static async Task<Results<NoContent, NotFound>> DeleteLineAsync(
        int lId, ApplicationDbContext db, CancellationToken ct)
    {
        var entity = await db.BomLines.FirstOrDefaultAsync(l => l.Id == lId, ct);
        if (entity is null) return TypedResults.NotFound();
        db.BomLines.Remove(entity);
        await db.SaveChangesAsync(ct);
        return TypedResults.NoContent();
    }
}
