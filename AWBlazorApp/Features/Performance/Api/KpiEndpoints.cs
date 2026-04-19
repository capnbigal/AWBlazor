using System.Security.Claims;
using AWBlazorApp.Features.Identity.Domain; using AWBlazorApp.Features.Admin.Permissions.Domain;
using AWBlazorApp.Features.Performance.Audit;
using AWBlazorApp.Features.Performance.Domain;
using AWBlazorApp.Features.Performance.Dtos;
using AWBlazorApp.Features.Performance.Services;
using AWBlazorApp.Infrastructure.Persistence;
using AWBlazorApp.Shared.Dtos;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.Performance.Api;

public static class KpiEndpoints
{
    public static IEndpointRouteBuilder MapKpiEndpoints(this IEndpointRouteBuilder app)
    {
        var defs = app.MapGroup("/api/kpi-definitions")
            .WithTags("KpiDefinitions")
            .RequireAuthorization("ApiOrCookie");

        defs.MapGet("/", ListDefsAsync).WithName("ListKpiDefinitions");
        defs.MapGet("/{id:int}", GetDefAsync).WithName("GetKpiDefinition");
        defs.MapPost("/", CreateDefAsync).WithName("CreateKpiDefinition")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));
        defs.MapPatch("/{id:int}", UpdateDefAsync).WithName("UpdateKpiDefinition")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));
        defs.MapDelete("/{id:int}", DeleteDefAsync).WithName("DeleteKpiDefinition")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Admin));
        defs.MapGet("/{id:int}/history", HistoryAsync).WithName("ListKpiDefinitionHistory");

        var values = app.MapGroup("/api/kpi-values")
            .WithTags("KpiValues")
            .RequireAuthorization("ApiOrCookie");

        values.MapGet("/", ListValuesAsync).WithName("ListKpiValues");
        values.MapPost("/evaluate", EvaluateAsync).WithName("EvaluateKpi")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));

        return app;
    }

    private static async Task<Ok<PagedResult<KpiDefinitionDto>>> ListDefsAsync(
        ApplicationDbContext db,
        [FromQuery] int skip = 0, [FromQuery] int take = 50,
        [FromQuery] KpiSource? source = null, [FromQuery] bool? isActive = null,
        CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 500);
        var q = db.KpiDefinitions.AsNoTracking();
        if (source.HasValue) q = q.Where(x => x.Source == source.Value);
        if (isActive.HasValue) q = q.Where(x => x.IsActive == isActive.Value);
        var total = await q.CountAsync(ct);
        var rows = await q.OrderBy(x => x.Code)
            .Skip(skip).Take(take).Select(x => x.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<KpiDefinitionDto>(rows, total, skip, take));
    }

    private static async Task<Results<Ok<KpiDefinitionDto>, NotFound>> GetDefAsync(int id, ApplicationDbContext db, CancellationToken ct)
    {
        var row = await db.KpiDefinitions.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        return row is null ? TypedResults.NotFound() : TypedResults.Ok(row.ToDto());
    }

    private static async Task<Results<Created<IdResponse>, ValidationProblem>> CreateDefAsync(
        CreateKpiDefinitionRequest request,
        IValidator<CreateKpiDefinitionRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());
        var entity = request.ToEntity();
        await db.AddWithAuditAsync(entity, e => KpiDefinitionAuditService.RecordCreate(e, user.Identity?.Name), ct);
        return TypedResults.Created($"/api/kpi-definitions/{entity.Id}", new IdResponse(entity.Id));
    }

    private static async Task<Results<Ok<IdResponse>, NotFound, ValidationProblem>> UpdateDefAsync(
        int id, UpdateKpiDefinitionRequest request,
        IValidator<UpdateKpiDefinitionRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());
        var entity = await db.KpiDefinitions.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null) return TypedResults.NotFound();
        var before = KpiDefinitionAuditService.CaptureSnapshot(entity);
        request.ApplyTo(entity);
        db.KpiDefinitionAuditLogs.Add(KpiDefinitionAuditService.RecordUpdate(before, entity, user.Identity?.Name));
        await db.SaveChangesAsync(ct);
        return TypedResults.Ok(new IdResponse(entity.Id));
    }

    private static async Task<Results<NoContent, NotFound>> DeleteDefAsync(
        int id, ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var entity = await db.KpiDefinitions.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null) return TypedResults.NotFound();
        await db.DeleteWithAuditAsync(entity, KpiDefinitionAuditService.RecordDelete(entity, user.Identity?.Name), ct);
        return TypedResults.NoContent();
    }

    private static async Task<Ok<PagedResult<KpiDefinitionAuditLogDto>>> HistoryAsync(
        int id, ApplicationDbContext db,
        [FromQuery] int skip = 0, [FromQuery] int take = 50, CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 500);
        var q = db.KpiDefinitionAuditLogs.AsNoTracking().Where(a => a.KpiDefinitionId == id);
        var total = await q.CountAsync(ct);
        var rows = await q.OrderByDescending(a => a.ChangedDate).ThenByDescending(a => a.Id)
            .Skip(skip).Take(take).Select(a => a.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<KpiDefinitionAuditLogDto>(rows, total, skip, take));
    }

    private static async Task<Ok<PagedResult<KpiValueDto>>> ListValuesAsync(
        ApplicationDbContext db,
        [FromQuery] int skip = 0, [FromQuery] int take = 50,
        [FromQuery] int? kpiDefinitionId = null,
        [FromQuery] PerformancePeriodKind? periodKind = null,
        CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 500);
        var q = db.KpiValues.AsNoTracking();
        if (kpiDefinitionId.HasValue) q = q.Where(x => x.KpiDefinitionId == kpiDefinitionId.Value);
        if (periodKind.HasValue) q = q.Where(x => x.PeriodKind == periodKind.Value);
        var total = await q.CountAsync(ct);
        var rows = await q.OrderByDescending(x => x.PeriodStart)
            .Skip(skip).Take(take).Select(x => x.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<KpiValueDto>(rows, total, skip, take));
    }

    private static async Task<Results<Ok<KpiValueDto>, NotFound, ValidationProblem>> EvaluateAsync(
        EvaluateKpiRequest request,
        IValidator<EvaluateKpiRequest> validator,
        IKpiEvaluationService svc, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());
        try
        {
            var value = await svc.EvaluateAsync(
                request.KpiDefinitionId, request.PeriodKind,
                request.PeriodStart, request.PeriodEnd, ct);
            return TypedResults.Ok(value.ToDto());
        }
        catch (KeyNotFoundException)
        {
            return TypedResults.NotFound();
        }
    }
}
