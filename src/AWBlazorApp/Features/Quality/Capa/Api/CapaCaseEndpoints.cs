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

namespace AWBlazorApp.Features.Quality.Capa.Api;

public static class CapaCaseEndpoints
{
    public static IEndpointRouteBuilder MapCapaCaseEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/capa-cases")
            .WithTags("CapaCases")
            .RequireAuthorization("ApiOrCookie");

        group.MapGet("/", ListAsync).WithName("ListCapaCases");
        group.MapGet("/{id:int}", GetAsync).WithName("GetCapaCase");
        group.MapPost("/", CreateAsync).WithName("CreateCapaCase")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));
        group.MapPatch("/{id:int}", UpdateAsync).WithName("UpdateCapaCase")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));
        group.MapPost("/{id:int}/transition", TransitionAsync).WithName("TransitionCapaCase")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));
        group.MapGet("/{id:int}/history", HistoryAsync).WithName("ListCapaCaseHistory");

        group.MapGet("/{id:int}/ncrs", ListLinkedNcrsAsync).WithName("ListCapaCaseLinkedNcrs");
        group.MapPost("/{id:int}/ncrs", LinkNcrAsync).WithName("LinkNcrToCapa")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));
        group.MapDelete("/{id:int}/ncrs/{ncrId:int}", UnlinkNcrAsync).WithName("UnlinkNcrFromCapa")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));

        return app;
    }

    private static async Task<Ok<PagedResult<CapaCaseDto>>> ListAsync(
        ApplicationDbContext db,
        [FromQuery] int skip = 0, [FromQuery] int take = 50,
        [FromQuery] CapaStatus? status = null,
        CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 500);
        var q = db.CapaCases.AsNoTracking();
        if (status.HasValue) q = q.Where(x => x.Status == status.Value);
        var total = await q.CountAsync(ct);
        var rows = await q.OrderByDescending(x => x.OpenedAt).ThenByDescending(x => x.Id)
            .Skip(skip).Take(take).Select(x => x.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<CapaCaseDto>(rows, total, skip, take));
    }

    private static async Task<Results<Ok<CapaCaseDto>, NotFound>> GetAsync(int id, ApplicationDbContext db, CancellationToken ct)
    {
        var row = await db.CapaCases.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        return row is null ? TypedResults.NotFound() : TypedResults.Ok(row.ToDto());
    }

    private static async Task<Results<Created<IdResponse>, ValidationProblem>> CreateAsync(
        CreateCapaCaseRequest request,
        IValidator<CreateCapaCaseRequest> validator,
        ICapaService svc, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());
        var id = await svc.OpenAsync(request.Title!, request.OwnerBusinessEntityId,
            request.LinkedNcrIds ?? [], user.Identity?.Name, ct);
        return TypedResults.Created($"/api/capa-cases/{id}", new IdResponse(id));
    }

    private static async Task<Results<Ok<IdResponse>, NotFound, ValidationProblem>> UpdateAsync(
        int id, UpdateCapaCaseRequest request,
        IValidator<UpdateCapaCaseRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());
        var entity = await db.CapaCases.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null) return TypedResults.NotFound();
        request.ApplyTo(entity);
        await db.SaveChangesAsync(ct);
        return TypedResults.Ok(new IdResponse(entity.Id));
    }

    private static async Task<Results<Ok<IdResponse>, BadRequest<string>>> TransitionAsync(
        int id, TransitionCapaCaseRequest request, ICapaService svc, ClaimsPrincipal user, CancellationToken ct)
    {
        try
        {
            await svc.TransitionAsync(id, request.TargetStatus, user.Identity?.Name, ct);
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
        var q = db.AuditLogs.AsNoTracking().Where(a => a.EntityType == "CapaCase" && a.EntityId == idStr);
        var total = await q.CountAsync(ct);
        var rows = await q.OrderByDescending(a => a.ChangedDate).ThenByDescending(a => a.Id)
            .Skip(skip).Take(take).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<AuditLog>(rows, total, skip, take));
    }

    private static async Task<Ok<PagedResult<CapaCaseNonConformanceDto>>> ListLinkedNcrsAsync(
        int id, ApplicationDbContext db, CancellationToken ct = default)
    {
        var rows = await db.CapaCaseNonConformances.AsNoTracking()
            .Where(l => l.CapaCaseId == id).OrderByDescending(l => l.LinkedAt)
            .Select(l => l.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<CapaCaseNonConformanceDto>(rows, rows.Count, 0, rows.Count));
    }

    private static async Task<Ok<IdResponse>> LinkNcrAsync(int id, LinkNcrRequest request, ICapaService svc, CancellationToken ct)
    {
        await svc.LinkNcrAsync(id, request.NonConformanceId, ct);
        return TypedResults.Ok(new IdResponse(id));
    }

    private static async Task<NoContent> UnlinkNcrAsync(int id, int ncrId, ICapaService svc, CancellationToken ct)
    {
        await svc.UnlinkNcrAsync(id, ncrId, ct);
        return TypedResults.NoContent();
    }
}
