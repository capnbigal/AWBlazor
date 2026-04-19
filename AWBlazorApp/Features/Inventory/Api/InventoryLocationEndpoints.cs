using System.Security.Claims;
using AWBlazorApp.Features.Identity.Domain; using AWBlazorApp.Features.Admin.Permissions.Domain;
using AWBlazorApp.Infrastructure.Persistence;
using AWBlazorApp.Shared.Dtos;
using AWBlazorApp.Features.Inventory.Audit;
using AWBlazorApp.Features.Inventory.Domain;
using AWBlazorApp.Features.Inventory.Dtos;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.Inventory.Api;

/// <summary>
/// Hand-rolled CRUD for <c>inv.InventoryLocation</c>. Mirrors the OrgUnit endpoints because
/// Create/Update need to resolve the materialized <c>Path</c> / <c>Depth</c> from the parent
/// row, and a parent change cascades new paths onto descendants.
/// </summary>
public static class InventoryLocationEndpoints
{
    public static IEndpointRouteBuilder MapInventoryLocationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/inventory-locations")
            .WithTags("InventoryLocations")
            .RequireAuthorization("ApiOrCookie");

        group.MapGet("/", ListAsync).WithName("ListInventoryLocations");
        group.MapGet("/{id:int}", GetAsync).WithName("GetInventoryLocation");
        group.MapPost("/", CreateAsync).WithName("CreateInventoryLocation")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));
        group.MapPatch("/{id:int}", UpdateAsync).WithName("UpdateInventoryLocation")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));
        group.MapDelete("/{id:int}", DeleteAsync).WithName("DeleteInventoryLocation")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));
        group.MapGet("/{id:int}/history", HistoryAsync).WithName("ListInventoryLocationHistory");

        return app;
    }

    private static async Task<Ok<PagedResult<InventoryLocationDto>>> ListAsync(
        ApplicationDbContext db,
        [FromQuery] int skip = 0, [FromQuery] int take = 100,
        [FromQuery] int? organizationId = null, [FromQuery] int? parentLocationId = null,
        [FromQuery] InventoryLocationKind? kind = null, [FromQuery] string? code = null,
        [FromQuery] bool? isActive = null,
        CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 1000);
        var query = db.InventoryLocations.AsNoTracking();
        if (organizationId.HasValue) query = query.Where(x => x.OrganizationId == organizationId.Value);
        if (parentLocationId.HasValue) query = query.Where(x => x.ParentLocationId == parentLocationId.Value);
        if (kind.HasValue) query = query.Where(x => x.Kind == kind.Value);
        if (!string.IsNullOrWhiteSpace(code)) query = query.Where(x => x.Code.Contains(code));
        if (isActive.HasValue) query = query.Where(x => x.IsActive == isActive.Value);
        var total = await query.CountAsync(ct);
        var rows = await query.OrderBy(x => x.Path).ThenBy(x => x.Code)
            .Skip(skip).Take(take).Select(x => x.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<InventoryLocationDto>(rows, total, skip, take));
    }

    private static async Task<Results<Ok<InventoryLocationDto>, NotFound>> GetAsync(
        int id, ApplicationDbContext db, CancellationToken ct)
    {
        var row = await db.InventoryLocations.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        return row is null ? TypedResults.NotFound() : TypedResults.Ok(row.ToDto());
    }

    private static async Task<Results<Created<IdResponse>, ValidationProblem>> CreateAsync(
        CreateInventoryLocationRequest request,
        IValidator<CreateInventoryLocationRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());

        var entity = request.ToEntity();
        await ResolvePathAndDepthAsync(db, entity, ct);

        await db.AddWithAuditAsync(entity, e => InventoryLocationAuditService.RecordCreate(e, user.Identity?.Name), ct);
        return TypedResults.Created($"/api/inventory-locations/{entity.Id}", new IdResponse(entity.Id));
    }

    private static async Task<Results<Ok<IdResponse>, NotFound, ValidationProblem>> UpdateAsync(
        int id, UpdateInventoryLocationRequest request,
        IValidator<UpdateInventoryLocationRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());

        var entity = await db.InventoryLocations.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null) return TypedResults.NotFound();

        var before = InventoryLocationAuditService.CaptureSnapshot(entity);
        var oldPath = entity.Path;
        request.ApplyTo(entity);

        var parentChanged = request.ParentLocationId is not null && request.ParentLocationId != before.ParentLocationId;
        var codeChanged = request.Code is not null && request.Code.Trim().ToUpperInvariant() != before.Code;
        if (parentChanged || codeChanged)
        {
            await ResolvePathAndDepthAsync(db, entity, ct);
            await CascadePathAsync(db, entity.Id, oldPath, entity.Path, entity.Depth, ct);
        }

        db.InventoryLocationAuditLogs.Add(InventoryLocationAuditService.RecordUpdate(before, entity, user.Identity?.Name));
        await db.SaveChangesAsync(ct);
        return TypedResults.Ok(new IdResponse(entity.Id));
    }

    private static async Task<Results<NoContent, NotFound, ValidationProblem>> DeleteAsync(
        int id, ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var entity = await db.InventoryLocations.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null) return TypedResults.NotFound();

        var hasChildren = await db.InventoryLocations.AnyAsync(x => x.ParentLocationId == id, ct);
        if (hasChildren)
        {
            return TypedResults.ValidationProblem(new Dictionary<string, string[]>
            {
                ["ParentLocationId"] = ["Cannot delete a location that has children. Reparent or delete them first."],
            });
        }

        await db.DeleteWithAuditAsync(entity, InventoryLocationAuditService.RecordDelete(entity, user.Identity?.Name), ct);
        return TypedResults.NoContent();
    }

    private static async Task<Ok<PagedResult<InventoryLocationAuditLogDto>>> HistoryAsync(
        int id, ApplicationDbContext db,
        [FromQuery] int skip = 0, [FromQuery] int take = 50, CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 500);
        var query = db.InventoryLocationAuditLogs.AsNoTracking().Where(x => x.InventoryLocationId == id);
        var total = await query.CountAsync(ct);
        var rows = await query.OrderByDescending(x => x.ChangedDate).ThenByDescending(x => x.Id)
            .Skip(skip).Take(take).Select(a => a.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<InventoryLocationAuditLogDto>(rows, total, skip, take));
    }

    private static async Task ResolvePathAndDepthAsync(ApplicationDbContext db, InventoryLocation entity, CancellationToken ct)
    {
        if (entity.ParentLocationId is null)
        {
            entity.Path = entity.Code;
            entity.Depth = 0;
            return;
        }
        var parent = await db.InventoryLocations.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == entity.ParentLocationId, ct)
            ?? throw new InvalidOperationException($"ParentLocationId {entity.ParentLocationId} does not exist.");
        entity.Path = string.IsNullOrEmpty(parent.Path) ? entity.Code : parent.Path + "/" + entity.Code;
        entity.Depth = (byte)(parent.Depth + 1);
    }

    private static async Task CascadePathAsync(
        ApplicationDbContext db, int rootId, string oldPath, string newPath, byte newDepth, CancellationToken ct)
    {
        if (oldPath == newPath) return;
        var descendants = await db.InventoryLocations
            .Where(x => x.Id != rootId && x.Path.StartsWith(oldPath + "/"))
            .ToListAsync(ct);
        foreach (var d in descendants)
        {
            d.Path = newPath + d.Path.Substring(oldPath.Length);
            d.Depth = (byte)(d.Depth - (oldPath.Count(c => c == '/') - newPath.Count(c => c == '/')));
        }
    }
}
