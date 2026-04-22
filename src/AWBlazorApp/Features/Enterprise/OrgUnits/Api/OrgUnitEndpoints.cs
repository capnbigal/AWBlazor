using System.Security.Claims;
using AWBlazorApp.Features.Identity.Domain; using AWBlazorApp.Features.Admin.Permissions.Domain;
using AWBlazorApp.Infrastructure.Persistence;
using AWBlazorApp.Shared.Audit;
using AWBlazorApp.Shared.Dtos;
using AWBlazorApp.Features.Enterprise.OrgUnits.Domain;
using AWBlazorApp.Features.Enterprise.OrgUnits.Dtos;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.Enterprise.OrgUnits.Api;

/// <summary>
/// OrgUnit endpoints. Written by hand (not via <c>MapCrudWithInterceptor</c>) because
/// Create/Update must resolve the materialized <c>Path</c> and <c>Depth</c> from the
/// parent row before persisting, and a parent change must cascade new paths onto
/// descendants. Audit rows are written automatically by
/// <see cref="AuditLogInterceptor"/> on <c>SaveChangesAsync</c>.
/// </summary>
public static class OrgUnitEndpoints
{
    public static IEndpointRouteBuilder MapOrgUnitEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/org-units")
            .WithTags("OrgUnits")
            .RequireAuthorization("ApiOrCookie");

        group.MapGet("/", ListAsync).WithName("ListOrgUnits");
        group.MapGet("/{id:int}", GetAsync).WithName("GetOrgUnit");
        group.MapPost("/", CreateAsync).WithName("CreateOrgUnit")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));
        group.MapPatch("/{id:int}", UpdateAsync).WithName("UpdateOrgUnit")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));
        group.MapDelete("/{id:int}", DeleteAsync).WithName("DeleteOrgUnit")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));
        group.MapGet("/{id:int}/history", HistoryAsync).WithName("ListOrgUnitHistory");

        return app;
    }

    private static async Task<Ok<PagedResult<OrgUnitDto>>> ListAsync(
        ApplicationDbContext db,
        [FromQuery] int skip = 0, [FromQuery] int take = 100,
        [FromQuery] int? organizationId = null, [FromQuery] int? parentOrgUnitId = null,
        [FromQuery] OrgUnitKind? kind = null, [FromQuery] string? code = null,
        [FromQuery] bool? isActive = null,
        CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 1000);
        var query = db.OrgUnits.AsNoTracking();
        if (organizationId.HasValue) query = query.Where(x => x.OrganizationId == organizationId.Value);
        if (parentOrgUnitId.HasValue) query = query.Where(x => x.ParentOrgUnitId == parentOrgUnitId.Value);
        if (kind.HasValue) query = query.Where(x => x.Kind == kind.Value);
        if (!string.IsNullOrWhiteSpace(code)) query = query.Where(x => x.Code.Contains(code));
        if (isActive.HasValue) query = query.Where(x => x.IsActive == isActive.Value);
        var total = await query.CountAsync(ct);
        var rows = await query.OrderBy(x => x.Path).ThenBy(x => x.Code)
            .Skip(skip).Take(take).Select(x => x.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<OrgUnitDto>(rows, total, skip, take));
    }

    private static async Task<Results<Ok<OrgUnitDto>, NotFound>> GetAsync(
        int id, ApplicationDbContext db, CancellationToken ct)
    {
        var row = await db.OrgUnits.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        return row is null ? TypedResults.NotFound() : TypedResults.Ok(row.ToDto());
    }

    private static async Task<Results<Created<IdResponse>, ValidationProblem>> CreateAsync(
        CreateOrgUnitRequest request,
        IValidator<CreateOrgUnitRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());

        var entity = request.ToEntity();
        await ResolvePathAndDepthAsync(db, entity, ct);

        db.OrgUnits.Add(entity);
        await db.SaveChangesAsync(ct);
        return TypedResults.Created($"/api/org-units/{entity.Id}", new IdResponse(entity.Id));
    }

    private static async Task<Results<Ok<IdResponse>, NotFound, ValidationProblem>> UpdateAsync(
        int id, UpdateOrgUnitRequest request,
        IValidator<UpdateOrgUnitRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());

        var entity = await db.OrgUnits.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null) return TypedResults.NotFound();

        // Capture the pre-Apply values we need for cascade logic. The interceptor
        // handles the actual audit-row write on SaveChangesAsync below.
        var beforeParent = entity.ParentOrgUnitId;
        var beforeCode = entity.Code;
        var oldPath = entity.Path;
        request.ApplyTo(entity);

        var parentChanged = request.ParentOrgUnitId is not null && request.ParentOrgUnitId != beforeParent;
        var codeChanged = request.Code is not null && request.Code.Trim().ToUpperInvariant() != beforeCode;
        if (parentChanged || codeChanged)
        {
            await ResolvePathAndDepthAsync(db, entity, ct);
            await CascadePathAsync(db, entity.Id, oldPath, entity.Path, entity.Depth, ct);
        }

        await db.SaveChangesAsync(ct);
        return TypedResults.Ok(new IdResponse(entity.Id));
    }

    private static async Task<Results<NoContent, NotFound, ValidationProblem>> DeleteAsync(
        int id, ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var entity = await db.OrgUnits.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null) return TypedResults.NotFound();

        var hasChildren = await db.OrgUnits.AnyAsync(x => x.ParentOrgUnitId == id, ct);
        if (hasChildren)
        {
            return TypedResults.ValidationProblem(new Dictionary<string, string[]>
            {
                ["OrgUnit"] = ["Cannot delete an OrgUnit with child units. Reparent or delete the children first."],
            });
        }

        db.OrgUnits.Remove(entity);
        await db.SaveChangesAsync(ct);
        return TypedResults.NoContent();
    }

    private static async Task<Ok<List<AuditLog>>> HistoryAsync(
        int id, ApplicationDbContext db, CancellationToken ct)
    {
        var idStr = id.ToString();
        var rows = await db.AuditLogs.AsNoTracking()
            .Where(a => a.EntityType == "OrgUnit" && a.EntityId == idStr)
            .OrderByDescending(a => a.ChangedDate).ThenByDescending(a => a.Id)
            .ToListAsync(ct);
        return TypedResults.Ok(rows);
    }

    private static async Task ResolvePathAndDepthAsync(ApplicationDbContext db, OrgUnit entity, CancellationToken ct)
    {
        if (entity.ParentOrgUnitId is null)
        {
            entity.Path = entity.Code;
            entity.Depth = 0;
            return;
        }

        var parent = await db.OrgUnits.AsNoTracking()
            .Where(x => x.Id == entity.ParentOrgUnitId.Value)
            .Select(x => new { x.Path, x.Depth, x.OrganizationId })
            .FirstOrDefaultAsync(ct)
            ?? throw new InvalidOperationException($"Parent OrgUnit {entity.ParentOrgUnitId} not found.");

        entity.OrganizationId = parent.OrganizationId; // children always inherit the org
        entity.Path = $"{parent.Path}/{entity.Code}";
        entity.Depth = (byte)(parent.Depth + 1);
    }

    /// <summary>
    /// After a node moves or renames, rewrite <c>Path</c> and <c>Depth</c> on every descendant.
    /// Uses a parameterized SQL UPDATE because the ancestor shift can touch many rows and we
    /// want one round-trip instead of loading the sub-tree into memory.
    /// </summary>
    private static async Task CascadePathAsync(
        ApplicationDbContext db, int rootId, string oldPath, string newPath, byte newRootDepth, CancellationToken ct)
    {
        if (oldPath == newPath) return;
        var prefix = oldPath + "/";
        var depthShift = newRootDepth - (byte)(oldPath.Count(c => c == '/'));
        await db.Database.ExecuteSqlInterpolatedAsync(
            $@"UPDATE [org].[OrgUnit]
               SET [Path] = {newPath} + SUBSTRING([Path], LEN({oldPath}) + 1, 8000),
                   [Depth] = [Depth] + {depthShift}
               WHERE [Id] <> {rootId} AND [Path] LIKE {prefix + "%"}", ct);
    }
}
