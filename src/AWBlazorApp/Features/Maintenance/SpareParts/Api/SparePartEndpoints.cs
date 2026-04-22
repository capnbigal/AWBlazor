using System.Security.Claims;
using AWBlazorApp.Features.Identity.Domain; using AWBlazorApp.Features.Admin.Permissions.Domain;
using AWBlazorApp.Features.Maintenance.AssetProfiles.Dtos; using AWBlazorApp.Features.Maintenance.PmSchedules.Dtos; using AWBlazorApp.Features.Maintenance.SpareParts.Dtos; using AWBlazorApp.Features.Maintenance.WorkOrders.Dtos;
using AWBlazorApp.Infrastructure.Persistence;
using AWBlazorApp.Shared.Audit;
using AWBlazorApp.Shared.Dtos;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.Maintenance.SpareParts.Api;

public static class SparePartEndpoints
{
    public static IEndpointRouteBuilder MapSparePartEndpoints(this IEndpointRouteBuilder app)
    {
        var parts = app.MapGroup("/api/spare-parts")
            .WithTags("SpareParts")
            .RequireAuthorization("ApiOrCookie");

        parts.MapGet("/", ListAsync).WithName("ListSpareParts");
        parts.MapGet("/{id:int}", GetAsync).WithName("GetSparePart");
        parts.MapPost("/", CreateAsync).WithName("CreateSparePart")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));
        parts.MapPatch("/{id:int}", UpdateAsync).WithName("UpdateSparePart")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));
        parts.MapDelete("/{id:int}", DeleteAsync).WithName("DeleteSparePart")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Admin));
        parts.MapGet("/{id:int}/history", HistoryAsync).WithName("ListSparePartHistory");

        parts.MapGet("/{id:int}/usage", ListUsageAsync).WithName("ListSparePartUsage");
        parts.MapPost("/usage", CreateUsageAsync).WithName("CreateSparePartUsage");

        return app;
    }

    private static async Task<Ok<PagedResult<SparePartDto>>> ListAsync(
        ApplicationDbContext db,
        [FromQuery] int skip = 0, [FromQuery] int take = 50,
        [FromQuery] string? search = null,
        [FromQuery] bool? isActive = null,
        CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 500);
        var q = db.SpareParts.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            q = q.Where(x => x.PartNumber.Contains(s) || x.Name.Contains(s));
        }
        if (isActive.HasValue) q = q.Where(x => x.IsActive == isActive.Value);
        var total = await q.CountAsync(ct);
        var rows = await q.OrderBy(x => x.PartNumber)
            .Skip(skip).Take(take).Select(x => x.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<SparePartDto>(rows, total, skip, take));
    }

    private static async Task<Results<Ok<SparePartDto>, NotFound>> GetAsync(int id, ApplicationDbContext db, CancellationToken ct)
    {
        var row = await db.SpareParts.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        return row is null ? TypedResults.NotFound() : TypedResults.Ok(row.ToDto());
    }

    private static async Task<Results<Created<IdResponse>, ValidationProblem>> CreateAsync(
        CreateSparePartRequest request,
        IValidator<CreateSparePartRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());
        var entity = request.ToEntity();
        db.SpareParts.Add(entity);
        await db.SaveChangesAsync(ct);
        return TypedResults.Created($"/api/spare-parts/{entity.Id}", new IdResponse(entity.Id));
    }

    private static async Task<Results<Ok<IdResponse>, NotFound, ValidationProblem>> UpdateAsync(
        int id, UpdateSparePartRequest request,
        IValidator<UpdateSparePartRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());
        var entity = await db.SpareParts.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null) return TypedResults.NotFound();
        request.ApplyTo(entity);
        await db.SaveChangesAsync(ct);
        return TypedResults.Ok(new IdResponse(entity.Id));
    }

    private static async Task<Results<NoContent, NotFound>> DeleteAsync(
        int id, ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var entity = await db.SpareParts.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null) return TypedResults.NotFound();
        db.SpareParts.Remove(entity);
        await db.SaveChangesAsync(ct);
        return TypedResults.NoContent();
    }

    private static async Task<Ok<PagedResult<AuditLog>>> HistoryAsync(
        int id, ApplicationDbContext db,
        [FromQuery] int skip = 0, [FromQuery] int take = 50, CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 500);
        var idStr = id.ToString();
        var q = db.AuditLogs.AsNoTracking().Where(a => a.EntityType == "SparePart" && a.EntityId == idStr);
        var total = await q.CountAsync(ct);
        var rows = await q.OrderByDescending(a => a.ChangedDate).ThenByDescending(a => a.Id)
            .Skip(skip).Take(take).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<AuditLog>(rows, total, skip, take));
    }

    private static async Task<Ok<PagedResult<WorkOrderPartUsageDto>>> ListUsageAsync(
        int id, ApplicationDbContext db,
        [FromQuery] int skip = 0, [FromQuery] int take = 50, CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 500);
        var q = db.WorkOrderPartUsages.AsNoTracking().Where(u => u.SparePartId == id);
        var total = await q.CountAsync(ct);
        var rows = await q.OrderByDescending(u => u.UsedAt)
            .Skip(skip).Take(take).Select(u => u.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<WorkOrderPartUsageDto>(rows, total, skip, take));
    }

    private static async Task<Results<Created<IdResponse>, ValidationProblem>> CreateUsageAsync(
        CreateWorkOrderPartUsageRequest request,
        IValidator<CreateWorkOrderPartUsageRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());
        var entity = request.ToEntity(user.Identity?.Name);
        db.WorkOrderPartUsages.Add(entity);
        await db.SaveChangesAsync(ct);
        return TypedResults.Created($"/api/spare-parts/usage/{entity.Id}", new IdResponse(entity.Id));
    }
}
