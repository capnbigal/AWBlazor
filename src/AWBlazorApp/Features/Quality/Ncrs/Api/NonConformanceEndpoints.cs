using System.Security.Claims;
using AWBlazorApp.Features.Identity.Domain; using AWBlazorApp.Features.Admin.Permissions.Domain;
using AWBlazorApp.Infrastructure.Persistence;
using AWBlazorApp.Shared.Audit;
using AWBlazorApp.Shared.Dtos;
using AWBlazorApp.Features.Quality.Capa.Domain; using AWBlazorApp.Features.Quality.Inspections.Domain; using AWBlazorApp.Features.Quality.Ncrs.Domain; using AWBlazorApp.Features.Quality.Plans.Domain; 
using AWBlazorApp.Features.Quality.Capa.Dtos; using AWBlazorApp.Features.Quality.Inspections.Dtos; using AWBlazorApp.Features.Quality.Ncrs.Dtos; using AWBlazorApp.Features.Quality.Plans.Dtos; 
using AWBlazorApp.Features.Quality.Capa.Application.Services; using AWBlazorApp.Features.Quality.Inspections.Application.Services; using AWBlazorApp.Features.Quality.Ncrs.Application.Services; 
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.Quality.Ncrs.Api;

public static class NonConformanceEndpoints
{
    public static IEndpointRouteBuilder MapNonConformanceEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/non-conformances")
            .WithTags("NonConformances")
            .RequireAuthorization("ApiOrCookie");

        group.MapGet("/", ListAsync).WithName("ListNonConformances");
        group.MapGet("/{id:int}", GetAsync).WithName("GetNonConformance");
        group.MapPost("/", CreateAsync).WithName("CreateNonConformance")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));
        group.MapPatch("/{id:int}", UpdateAsync).WithName("UpdateNonConformance")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));
        group.MapPost("/{id:int}/disposition", DispositionAsync).WithName("DispositionNonConformance")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));
        group.MapGet("/{id:int}/history", HistoryAsync).WithName("ListNonConformanceHistory");

        group.MapGet("/{id:int}/actions", ListActionsAsync).WithName("ListNonConformanceActions");
        group.MapPost("/{id:int}/actions", AddActionAsync).WithName("CreateNonConformanceAction")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));

        return app;
    }

    private static async Task<Ok<PagedResult<NonConformanceDto>>> ListAsync(
        ApplicationDbContext db,
        [FromQuery] int skip = 0, [FromQuery] int take = 50,
        [FromQuery] NonConformanceStatus? status = null,
        [FromQuery] int? inventoryItemId = null,
        [FromQuery] NonConformanceDisposition? disposition = null,
        CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 500);
        var q = db.NonConformances.AsNoTracking();
        if (status.HasValue) q = q.Where(x => x.Status == status.Value);
        if (inventoryItemId.HasValue) q = q.Where(x => x.InventoryItemId == inventoryItemId.Value);
        if (disposition.HasValue) q = q.Where(x => x.Disposition == disposition.Value);
        var total = await q.CountAsync(ct);
        var rows = await q.OrderByDescending(x => x.ModifiedDate).ThenByDescending(x => x.Id)
            .Skip(skip).Take(take).Select(x => x.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<NonConformanceDto>(rows, total, skip, take));
    }

    private static async Task<Results<Ok<NonConformanceDto>, NotFound>> GetAsync(int id, ApplicationDbContext db, CancellationToken ct)
    {
        var row = await db.NonConformances.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        return row is null ? TypedResults.NotFound() : TypedResults.Ok(row.ToDto());
    }

    private static async Task<Results<Created<IdResponse>, ValidationProblem>> CreateAsync(
        CreateNonConformanceRequest request,
        IValidator<CreateNonConformanceRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());
        var entity = request.ToEntity();
        return TypedResults.Created($"/api/non-conformances/{entity.Id}", new IdResponse(entity.Id));
    }

    private static async Task<Results<Ok<IdResponse>, NotFound, ValidationProblem>> UpdateAsync(
        int id, UpdateNonConformanceRequest request,
        IValidator<UpdateNonConformanceRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());
        var entity = await db.NonConformances.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null) return TypedResults.NotFound();
        if (entity.Status == NonConformanceStatus.Closed)
            return TypedResults.ValidationProblem(new Dictionary<string, string[]> { ["Status"] = ["NCR is closed; no further edits allowed."] });
        request.ApplyTo(entity);
        await db.SaveChangesAsync(ct);
        return TypedResults.Ok(new IdResponse(entity.Id));
    }

    private static async Task<Results<Ok<IdResponse>, BadRequest<string>>> DispositionAsync(
        int id, DispositionNonConformanceRequest request,
        IValidator<DispositionNonConformanceRequest> validator,
        INonConformanceService svc, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.BadRequest(string.Join("; ", v.Errors.Select(e => e.ErrorMessage)));
        try
        {
            await svc.DispositionAsync(id, request.Disposition, request.Notes, user.Identity?.Name, ct);
            return TypedResults.Ok(new IdResponse(id));
        }
        catch (InvalidOperationException ex) { return TypedResults.BadRequest(ex.Message); }
    }

    private static async Task<Ok<PagedResult<AuditLog>>> HistoryAsync(
        int id, ApplicationDbContext db,
        [FromQuery] int skip = 0, [FromQuery] int take = 50, CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 500);
        var idStr = id.ToString();
        var q = db.AuditLogs.AsNoTracking().Where(a => a.EntityType == "NonConformance" && a.EntityId == idStr);
        var total = await q.CountAsync(ct);
        var rows = await q.OrderByDescending(a => a.ChangedDate).ThenByDescending(a => a.Id)
            .Skip(skip).Take(take).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<AuditLog>(rows, total, skip, take));
    }

    private static async Task<Ok<PagedResult<NonConformanceActionDto>>> ListActionsAsync(
        int id, ApplicationDbContext db, CancellationToken ct = default)
    {
        var rows = await db.NonConformanceActions.AsNoTracking()
            .Where(a => a.NonConformanceId == id)
            .OrderByDescending(a => a.PerformedAt)
            .Select(a => a.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<NonConformanceActionDto>(rows, rows.Count, 0, rows.Count));
    }

    private static async Task<Results<Created<IdResponse>, NotFound, ValidationProblem>> AddActionAsync(
        int id, CreateNonConformanceActionRequest request,
        IValidator<CreateNonConformanceActionRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());
        var ncr = await db.NonConformances.AsNoTracking().FirstOrDefaultAsync(n => n.Id == id, ct);
        if (ncr is null) return TypedResults.NotFound();
        request = request with { NonConformanceId = id };
        var entity = request.ToEntity(user.Identity?.Name);
        db.NonConformanceActions.Add(entity);
        await db.SaveChangesAsync(ct);
        await db.SaveChangesAsync(ct);
        return TypedResults.Created($"/api/non-conformances/{id}/actions/{entity.Id}", new IdResponse(entity.Id));
    }
}
