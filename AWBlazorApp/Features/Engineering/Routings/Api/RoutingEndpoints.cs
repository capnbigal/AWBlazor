using System.Security.Claims;
using AWBlazorApp.Features.Identity.Domain; using AWBlazorApp.Features.Admin.Permissions.Domain;
using AWBlazorApp.Features.Engineering.Audit;
using AWBlazorApp.Features.Engineering.Boms.Dtos; using AWBlazorApp.Features.Engineering.Ecos.Dtos; using AWBlazorApp.Features.Engineering.Routings.Dtos; 
using AWBlazorApp.Infrastructure.Persistence;
using AWBlazorApp.Shared.Dtos;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.Engineering.Routings.Api;

public static class RoutingEndpoints
{
    public static IEndpointRouteBuilder MapRoutingEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/manufacturing-routings")
            .WithTags("ManufacturingRoutings")
            .RequireAuthorization("ApiOrCookie");

        group.MapGet("/", ListAsync).WithName("ListManufacturingRoutings");
        group.MapGet("/{id:int}", GetAsync).WithName("GetManufacturingRouting");
        group.MapPost("/", CreateAsync).WithName("CreateManufacturingRouting")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));
        group.MapPatch("/{id:int}", UpdateAsync).WithName("UpdateManufacturingRouting")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));
        group.MapDelete("/{id:int}", DeleteAsync).WithName("DeleteManufacturingRouting")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Admin));
        group.MapGet("/{id:int}/history", HistoryAsync).WithName("ListManufacturingRoutingHistory");

        group.MapGet("/{id:int}/steps", ListStepsAsync).WithName("ListRoutingSteps");
        group.MapPost("/{id:int}/steps", CreateStepAsync).WithName("CreateRoutingStep")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));
        group.MapPatch("/steps/{sId:int}", UpdateStepAsync).WithName("UpdateRoutingStep")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));
        group.MapDelete("/steps/{sId:int}", DeleteStepAsync).WithName("DeleteRoutingStep")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));

        return app;
    }

    private static async Task<Ok<PagedResult<ManufacturingRoutingDto>>> ListAsync(
        ApplicationDbContext db,
        [FromQuery] int skip = 0, [FromQuery] int take = 50,
        [FromQuery] int? productId = null, [FromQuery] bool? isActive = null,
        CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 500);
        var q = db.ManufacturingRoutings.AsNoTracking();
        if (productId.HasValue) q = q.Where(x => x.ProductId == productId.Value);
        if (isActive.HasValue) q = q.Where(x => x.IsActive == isActive.Value);
        var total = await q.CountAsync(ct);
        var rows = await q.OrderBy(x => x.Code)
            .Skip(skip).Take(take).Select(x => x.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<ManufacturingRoutingDto>(rows, total, skip, take));
    }

    private static async Task<Results<Ok<ManufacturingRoutingDto>, NotFound>> GetAsync(int id, ApplicationDbContext db, CancellationToken ct)
    {
        var row = await db.ManufacturingRoutings.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        return row is null ? TypedResults.NotFound() : TypedResults.Ok(row.ToDto());
    }

    private static async Task<Results<Created<IdResponse>, ValidationProblem>> CreateAsync(
        CreateManufacturingRoutingRequest request,
        IValidator<CreateManufacturingRoutingRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());
        var entity = request.ToEntity();
        await db.AddWithAuditAsync(entity, e => ManufacturingRoutingAuditService.RecordCreate(e, user.Identity?.Name), ct);
        return TypedResults.Created($"/api/manufacturing-routings/{entity.Id}", new IdResponse(entity.Id));
    }

    private static async Task<Results<Ok<IdResponse>, NotFound, ValidationProblem>> UpdateAsync(
        int id, UpdateManufacturingRoutingRequest request,
        IValidator<UpdateManufacturingRoutingRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());
        var entity = await db.ManufacturingRoutings.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null) return TypedResults.NotFound();
        var before = ManufacturingRoutingAuditService.CaptureSnapshot(entity);
        request.ApplyTo(entity);
        db.ManufacturingRoutingAuditLogs.Add(ManufacturingRoutingAuditService.RecordUpdate(before, entity, user.Identity?.Name));
        await db.SaveChangesAsync(ct);
        return TypedResults.Ok(new IdResponse(entity.Id));
    }

    private static async Task<Results<NoContent, NotFound>> DeleteAsync(
        int id, ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var entity = await db.ManufacturingRoutings.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null) return TypedResults.NotFound();
        await db.DeleteWithAuditAsync(entity, ManufacturingRoutingAuditService.RecordDelete(entity, user.Identity?.Name), ct);
        return TypedResults.NoContent();
    }

    private static async Task<Ok<PagedResult<ManufacturingRoutingAuditLogDto>>> HistoryAsync(
        int id, ApplicationDbContext db,
        [FromQuery] int skip = 0, [FromQuery] int take = 50, CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 500);
        var q = db.ManufacturingRoutingAuditLogs.AsNoTracking().Where(a => a.ManufacturingRoutingId == id);
        var total = await q.CountAsync(ct);
        var rows = await q.OrderByDescending(a => a.ChangedDate).ThenByDescending(a => a.Id)
            .Skip(skip).Take(take).Select(a => a.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<ManufacturingRoutingAuditLogDto>(rows, total, skip, take));
    }

    private static async Task<Ok<PagedResult<RoutingStepDto>>> ListStepsAsync(
        int id, ApplicationDbContext db, CancellationToken ct = default)
    {
        var rows = await db.RoutingSteps.AsNoTracking()
            .Where(s => s.ManufacturingRoutingId == id)
            .OrderBy(s => s.SequenceNumber)
            .Select(s => s.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<RoutingStepDto>(rows, rows.Count, 0, rows.Count));
    }

    private static async Task<Results<Created<IdResponse>, NotFound, ValidationProblem>> CreateStepAsync(
        int id, CreateRoutingStepRequest request,
        IValidator<CreateRoutingStepRequest> validator,
        ApplicationDbContext db, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());
        var parent = await db.ManufacturingRoutings.AsNoTracking().FirstOrDefaultAsync(r => r.Id == id, ct);
        if (parent is null) return TypedResults.NotFound();
        request = request with { ManufacturingRoutingId = id };
        var entity = request.ToEntity();
        db.RoutingSteps.Add(entity);
        await db.SaveChangesAsync(ct);
        return TypedResults.Created($"/api/manufacturing-routings/{id}/steps/{entity.Id}", new IdResponse(entity.Id));
    }

    private static async Task<Results<Ok<IdResponse>, NotFound>> UpdateStepAsync(
        int sId, UpdateRoutingStepRequest request, ApplicationDbContext db, CancellationToken ct)
    {
        var entity = await db.RoutingSteps.FirstOrDefaultAsync(s => s.Id == sId, ct);
        if (entity is null) return TypedResults.NotFound();
        request.ApplyTo(entity);
        await db.SaveChangesAsync(ct);
        return TypedResults.Ok(new IdResponse(entity.Id));
    }

    private static async Task<Results<NoContent, NotFound>> DeleteStepAsync(
        int sId, ApplicationDbContext db, CancellationToken ct)
    {
        var entity = await db.RoutingSteps.FirstOrDefaultAsync(s => s.Id == sId, ct);
        if (entity is null) return TypedResults.NotFound();
        db.RoutingSteps.Remove(entity);
        await db.SaveChangesAsync(ct);
        return TypedResults.NoContent();
    }
}
