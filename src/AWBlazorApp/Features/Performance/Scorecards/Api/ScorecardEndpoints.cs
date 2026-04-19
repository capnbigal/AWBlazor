using System.Security.Claims;
using AWBlazorApp.Features.Identity.Domain; using AWBlazorApp.Features.Admin.Permissions.Domain;
using AWBlazorApp.Features.Performance.Audit;
using AWBlazorApp.Features.Performance.Kpis.Dtos; using AWBlazorApp.Features.Performance.ProductionMetrics.Dtos; using AWBlazorApp.Features.Performance.Scorecards.Dtos; 
using AWBlazorApp.Infrastructure.Persistence;
using AWBlazorApp.Shared.Dtos;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.Performance.Scorecards.Api;

public static class ScorecardEndpoints
{
    public static IEndpointRouteBuilder MapScorecardEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/scorecards")
            .WithTags("Scorecards")
            .RequireAuthorization("ApiOrCookie");

        group.MapGet("/", ListAsync).WithName("ListScorecards");
        group.MapGet("/{id:int}", GetAsync).WithName("GetScorecard");
        group.MapPost("/", CreateAsync).WithName("CreateScorecard")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));
        group.MapPatch("/{id:int}", UpdateAsync).WithName("UpdateScorecard")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));
        group.MapDelete("/{id:int}", DeleteAsync).WithName("DeleteScorecard")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Admin));
        group.MapGet("/{id:int}/history", HistoryAsync).WithName("ListScorecardHistory");

        group.MapGet("/{id:int}/kpis", ListKpisAsync).WithName("ListScorecardKpis");
        group.MapPost("/{id:int}/kpis", AddKpiAsync).WithName("AddScorecardKpi")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));
        group.MapDelete("/kpis/{kId:int}", RemoveKpiAsync).WithName("RemoveScorecardKpi")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));

        return app;
    }

    private static async Task<Ok<PagedResult<ScorecardDefinitionDto>>> ListAsync(
        ApplicationDbContext db,
        [FromQuery] int skip = 0, [FromQuery] int take = 50,
        [FromQuery] bool? isActive = null,
        CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 500);
        var q = db.ScorecardDefinitions.AsNoTracking();
        if (isActive.HasValue) q = q.Where(x => x.IsActive == isActive.Value);
        var total = await q.CountAsync(ct);
        var rows = await q.OrderBy(x => x.Code)
            .Skip(skip).Take(take).Select(x => x.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<ScorecardDefinitionDto>(rows, total, skip, take));
    }

    private static async Task<Results<Ok<ScorecardDefinitionDto>, NotFound>> GetAsync(int id, ApplicationDbContext db, CancellationToken ct)
    {
        var row = await db.ScorecardDefinitions.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        return row is null ? TypedResults.NotFound() : TypedResults.Ok(row.ToDto());
    }

    private static async Task<Results<Created<IdResponse>, ValidationProblem>> CreateAsync(
        CreateScorecardDefinitionRequest request,
        IValidator<CreateScorecardDefinitionRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());
        var entity = request.ToEntity(user.Identity?.Name);
        await db.AddWithAuditAsync(entity, e => ScorecardDefinitionAuditService.RecordCreate(e, user.Identity?.Name), ct);
        return TypedResults.Created($"/api/scorecards/{entity.Id}", new IdResponse(entity.Id));
    }

    private static async Task<Results<Ok<IdResponse>, NotFound, ValidationProblem>> UpdateAsync(
        int id, UpdateScorecardDefinitionRequest request,
        IValidator<UpdateScorecardDefinitionRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());
        var entity = await db.ScorecardDefinitions.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null) return TypedResults.NotFound();
        var before = ScorecardDefinitionAuditService.CaptureSnapshot(entity);
        request.ApplyTo(entity);
        db.ScorecardDefinitionAuditLogs.Add(ScorecardDefinitionAuditService.RecordUpdate(before, entity, user.Identity?.Name));
        await db.SaveChangesAsync(ct);
        return TypedResults.Ok(new IdResponse(entity.Id));
    }

    private static async Task<Results<NoContent, NotFound>> DeleteAsync(
        int id, ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var entity = await db.ScorecardDefinitions.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null) return TypedResults.NotFound();
        await db.DeleteWithAuditAsync(entity, ScorecardDefinitionAuditService.RecordDelete(entity, user.Identity?.Name), ct);
        return TypedResults.NoContent();
    }

    private static async Task<Ok<PagedResult<ScorecardDefinitionAuditLogDto>>> HistoryAsync(
        int id, ApplicationDbContext db,
        [FromQuery] int skip = 0, [FromQuery] int take = 50, CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 500);
        var q = db.ScorecardDefinitionAuditLogs.AsNoTracking().Where(a => a.ScorecardDefinitionId == id);
        var total = await q.CountAsync(ct);
        var rows = await q.OrderByDescending(a => a.ChangedDate).ThenByDescending(a => a.Id)
            .Skip(skip).Take(take).Select(a => a.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<ScorecardDefinitionAuditLogDto>(rows, total, skip, take));
    }

    private static async Task<Ok<PagedResult<ScorecardKpiDto>>> ListKpisAsync(
        int id, ApplicationDbContext db, CancellationToken ct = default)
    {
        var rows = await db.ScorecardKpis.AsNoTracking()
            .Where(k => k.ScorecardDefinitionId == id)
            .OrderBy(k => k.DisplayOrder)
            .Select(k => k.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<ScorecardKpiDto>(rows, rows.Count, 0, rows.Count));
    }

    private static async Task<Results<Created<IdResponse>, NotFound, Conflict<string>, ValidationProblem>> AddKpiAsync(
        int id, CreateScorecardKpiRequest request,
        IValidator<CreateScorecardKpiRequest> validator,
        ApplicationDbContext db, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());
        var parent = await db.ScorecardDefinitions.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id, ct);
        if (parent is null) return TypedResults.NotFound();
        request = request with { ScorecardDefinitionId = id };
        var exists = await db.ScorecardKpis.AnyAsync(
            k => k.ScorecardDefinitionId == id && k.KpiDefinitionId == request.KpiDefinitionId, ct);
        if (exists) return TypedResults.Conflict("This KPI is already on the scorecard.");
        var entity = request.ToEntity();
        db.ScorecardKpis.Add(entity);
        await db.SaveChangesAsync(ct);
        return TypedResults.Created($"/api/scorecards/{id}/kpis/{entity.Id}", new IdResponse(entity.Id));
    }

    private static async Task<Results<NoContent, NotFound>> RemoveKpiAsync(
        int kId, ApplicationDbContext db, CancellationToken ct)
    {
        var entity = await db.ScorecardKpis.FirstOrDefaultAsync(k => k.Id == kId, ct);
        if (entity is null) return TypedResults.NotFound();
        db.ScorecardKpis.Remove(entity);
        await db.SaveChangesAsync(ct);
        return TypedResults.NoContent();
    }
}
