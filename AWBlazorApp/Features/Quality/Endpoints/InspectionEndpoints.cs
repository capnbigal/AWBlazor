using System.Security.Claims;
using AWBlazorApp.Data;
using AWBlazorApp.Infrastructure.Persistence;
using AWBlazorApp.Shared.Models;
using AWBlazorApp.Features.Quality.Audit;
using AWBlazorApp.Features.Quality.Domain;
using AWBlazorApp.Features.Quality.Models;
using AWBlazorApp.Features.Quality.Services;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.Quality.Endpoints;

public static class InspectionEndpoints
{
    public static IEndpointRouteBuilder MapInspectionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/inspections")
            .WithTags("Inspections")
            .RequireAuthorization("ApiOrCookie");

        group.MapGet("/", ListAsync).WithName("ListInspections");
        group.MapGet("/{id:int}", GetAsync).WithName("GetInspection");
        group.MapPost("/", CreateAsync).WithName("CreateInspection")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));
        group.MapPost("/{id:int}/start", StartAsync).WithName("StartInspection")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));
        group.MapPost("/{id:int}/complete", CompleteAsync).WithName("CompleteInspection")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));
        group.MapGet("/{id:int}/results", ListResultsAsync).WithName("ListInspectionResults");
        group.MapPost("/{id:int}/results", RecordResultAsync).WithName("RecordInspectionResult")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));
        group.MapGet("/{id:int}/history", HistoryAsync).WithName("ListInspectionHistory");

        return app;
    }

    private static async Task<Ok<PagedResult<InspectionDto>>> ListAsync(
        ApplicationDbContext db,
        [FromQuery] int skip = 0, [FromQuery] int take = 50,
        [FromQuery] InspectionStatus? status = null,
        [FromQuery] InspectionSourceKind? sourceKind = null,
        [FromQuery] int? inspectionPlanId = null,
        CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 500);
        var q = db.Inspections.AsNoTracking();
        if (status.HasValue) q = q.Where(x => x.Status == status.Value);
        if (sourceKind.HasValue) q = q.Where(x => x.SourceKind == sourceKind.Value);
        if (inspectionPlanId.HasValue) q = q.Where(x => x.InspectionPlanId == inspectionPlanId.Value);
        var total = await q.CountAsync(ct);
        var rows = await q.OrderByDescending(x => x.ModifiedDate).ThenByDescending(x => x.Id)
            .Skip(skip).Take(take).Select(x => x.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<InspectionDto>(rows, total, skip, take));
    }

    private static async Task<Results<Ok<InspectionDto>, NotFound>> GetAsync(int id, ApplicationDbContext db, CancellationToken ct)
    {
        var row = await db.Inspections.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        return row is null ? TypedResults.NotFound() : TypedResults.Ok(row.ToDto());
    }

    private static async Task<Results<Created<IdResponse>, ValidationProblem, BadRequest<string>>> CreateAsync(
        CreateInspectionRequest request,
        IValidator<CreateInspectionRequest> validator,
        IInspectionService svc, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());
        try
        {
            var id = await svc.CreateAsync(new CreateInspectionInput(
                request.InspectionPlanId, request.SourceKind, request.SourceId,
                request.InventoryItemId, request.LotId, request.Quantity,
                request.UnitMeasureCode, request.Notes), user.Identity?.Name, ct);
            return TypedResults.Created($"/api/inspections/{id}", new IdResponse(id));
        }
        catch (InvalidOperationException ex) { return TypedResults.BadRequest(ex.Message); }
    }

    private static async Task<Results<Ok<IdResponse>, BadRequest<string>>> StartAsync(
        int id, StartInspectionRequest request, IInspectionService svc, ClaimsPrincipal user, CancellationToken ct)
    {
        try
        {
            await svc.StartAsync(id, request.InspectorBusinessEntityId, user.Identity?.Name, ct);
            return TypedResults.Ok(new IdResponse(id));
        }
        catch (InvalidOperationException ex) { return TypedResults.BadRequest(ex.Message); }
    }

    private static async Task<Results<Ok<CompleteResult>, BadRequest<string>>> CompleteAsync(
        int id, IInspectionService svc, ClaimsPrincipal user, CancellationToken ct)
    {
        try { return TypedResults.Ok(await svc.CompleteAsync(id, user.Identity?.Name, ct)); }
        catch (InvalidOperationException ex) { return TypedResults.BadRequest(ex.Message); }
    }

    private static async Task<Ok<PagedResult<InspectionResultDto>>> ListResultsAsync(
        int id, ApplicationDbContext db, CancellationToken ct = default)
    {
        var rows = await db.InspectionResults.AsNoTracking()
            .Where(r => r.InspectionId == id)
            .OrderBy(r => r.RecordedAt)
            .Select(r => r.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<InspectionResultDto>(rows, rows.Count, 0, rows.Count));
    }

    private static async Task<Results<Created<IdResponse>, ValidationProblem, BadRequest<string>>> RecordResultAsync(
        int id, RecordInspectionResultRequest request,
        IValidator<RecordInspectionResultRequest> validator,
        IInspectionService svc, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());
        try
        {
            var result = await svc.RecordResultAsync(new RecordResultInput(
                id, request.InspectionPlanCharacteristicId,
                request.NumericResult, request.AttributeResult, request.Notes),
                request.RecordedByBusinessEntityId, ct);
            return TypedResults.Created($"/api/inspections/{id}/results/{result.Id}", new IdResponse((int)result.Id));
        }
        catch (InvalidOperationException ex) { return TypedResults.BadRequest(ex.Message); }
    }

    private static async Task<Ok<PagedResult<InspectionAuditLogDto>>> HistoryAsync(
        int id, ApplicationDbContext db,
        [FromQuery] int skip = 0, [FromQuery] int take = 50, CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 500);
        var q = db.InspectionAuditLogs.AsNoTracking().Where(a => a.InspectionId == id);
        var total = await q.CountAsync(ct);
        var rows = await q.OrderByDescending(a => a.ChangedDate).ThenByDescending(a => a.Id)
            .Skip(skip).Take(take).Select(a => a.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<InspectionAuditLogDto>(rows, total, skip, take));
    }
}
