using System.Security.Claims;
using AWBlazorApp.Features.Identity.Domain; using AWBlazorApp.Features.Admin.Permissions.Domain;
using AWBlazorApp.Infrastructure.Persistence;
using AWBlazorApp.Shared.Audit;
using AWBlazorApp.Shared.Dtos;
using AWBlazorApp.Features.Quality.Capa.Domain; using AWBlazorApp.Features.Quality.Inspections.Domain; using AWBlazorApp.Features.Quality.Ncrs.Domain; using AWBlazorApp.Features.Quality.Plans.Domain; 
using AWBlazorApp.Features.Quality.Capa.Dtos; using AWBlazorApp.Features.Quality.Inspections.Dtos; using AWBlazorApp.Features.Quality.Ncrs.Dtos; using AWBlazorApp.Features.Quality.Plans.Dtos; 
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.Quality.Plans.Api;

public static class InspectionPlanEndpoints
{
    public static IEndpointRouteBuilder MapInspectionPlanEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/inspection-plans")
            .WithTags("InspectionPlans")
            .RequireAuthorization("ApiOrCookie");

        group.MapGet("/", ListAsync).WithName("ListInspectionPlans");
        group.MapGet("/{id:int}", GetAsync).WithName("GetInspectionPlan");
        group.MapPost("/", CreateAsync).WithName("CreateInspectionPlan")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));
        group.MapPatch("/{id:int}", UpdateAsync).WithName("UpdateInspectionPlan")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));
        group.MapDelete("/{id:int}", DeleteAsync).WithName("DeleteInspectionPlan")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Admin));
        group.MapGet("/{id:int}/history", HistoryAsync).WithName("ListInspectionPlanHistory");

        group.MapGet("/{id:int}/characteristics", ListCharsAsync).WithName("ListInspectionPlanCharacteristics");
        group.MapPost("/{id:int}/characteristics", CreateCharAsync).WithName("CreateInspectionPlanCharacteristic")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));
        group.MapPatch("/characteristics/{cId:int}", UpdateCharAsync).WithName("UpdateInspectionPlanCharacteristic")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));
        group.MapDelete("/characteristics/{cId:int}", DeleteCharAsync).WithName("DeleteInspectionPlanCharacteristic")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));

        return app;
    }

    private static async Task<Ok<PagedResult<InspectionPlanDto>>> ListAsync(
        ApplicationDbContext db,
        [FromQuery] int skip = 0, [FromQuery] int take = 50,
        [FromQuery] InspectionScope? scope = null,
        [FromQuery] int? productId = null,
        [FromQuery] int? vendorBusinessEntityId = null,
        [FromQuery] bool? isActive = null,
        CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 500);
        var q = db.InspectionPlans.AsNoTracking();
        if (scope.HasValue) q = q.Where(x => x.Scope == scope.Value);
        if (productId.HasValue) q = q.Where(x => x.ProductId == productId.Value);
        if (vendorBusinessEntityId.HasValue) q = q.Where(x => x.VendorBusinessEntityId == vendorBusinessEntityId.Value);
        if (isActive.HasValue) q = q.Where(x => x.IsActive == isActive.Value);
        var total = await q.CountAsync(ct);
        var rows = await q.OrderBy(x => x.PlanCode)
            .Skip(skip).Take(take).Select(x => x.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<InspectionPlanDto>(rows, total, skip, take));
    }

    private static async Task<Results<Ok<InspectionPlanDto>, NotFound>> GetAsync(int id, ApplicationDbContext db, CancellationToken ct)
    {
        var row = await db.InspectionPlans.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        return row is null ? TypedResults.NotFound() : TypedResults.Ok(row.ToDto());
    }

    private static async Task<Results<Created<IdResponse>, ValidationProblem>> CreateAsync(
        CreateInspectionPlanRequest request,
        IValidator<CreateInspectionPlanRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());
        var entity = request.ToEntity();
        return TypedResults.Created($"/api/inspection-plans/{entity.Id}", new IdResponse(entity.Id));
    }

    private static async Task<Results<Ok<IdResponse>, NotFound, ValidationProblem>> UpdateAsync(
        int id, UpdateInspectionPlanRequest request,
        IValidator<UpdateInspectionPlanRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());
        var entity = await db.InspectionPlans.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null) return TypedResults.NotFound();
        request.ApplyTo(entity);
        await db.SaveChangesAsync(ct);
        return TypedResults.Ok(new IdResponse(entity.Id));
    }

    private static async Task<Results<NoContent, NotFound>> DeleteAsync(
        int id, ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var entity = await db.InspectionPlans.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null) return TypedResults.NotFound();
        return TypedResults.NoContent();
    }

    private static async Task<Ok<PagedResult<AuditLog>>> HistoryAsync(
        int id, ApplicationDbContext db,
        [FromQuery] int skip = 0, [FromQuery] int take = 50, CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 500);
        var idStr = id.ToString();
        var q = db.AuditLogs.AsNoTracking().Where(a => a.EntityType == "InspectionPlan" && a.EntityId == idStr);
        var total = await q.CountAsync(ct);
        var rows = await q.OrderByDescending(a => a.ChangedDate).ThenByDescending(a => a.Id)
            .Skip(skip).Take(take).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<AuditLog>(rows, total, skip, take));
    }

    private static async Task<Ok<PagedResult<InspectionPlanCharacteristicDto>>> ListCharsAsync(
        int id, ApplicationDbContext db, CancellationToken ct = default)
    {
        var rows = await db.InspectionPlanCharacteristics.AsNoTracking()
            .Where(c => c.InspectionPlanId == id)
            .OrderBy(c => c.SequenceNumber)
            .Select(c => c.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<InspectionPlanCharacteristicDto>(rows, rows.Count, 0, rows.Count));
    }

    private static async Task<Results<Created<IdResponse>, NotFound, ValidationProblem>> CreateCharAsync(
        int id, CreateInspectionPlanCharacteristicRequest request,
        IValidator<CreateInspectionPlanCharacteristicRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());
        var plan = await db.InspectionPlans.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, ct);
        if (plan is null) return TypedResults.NotFound();
        request = request with { InspectionPlanId = id };
        var entity = request.ToEntity();
        db.InspectionPlanCharacteristics.Add(entity);
        await db.SaveChangesAsync(ct);
        await db.SaveChangesAsync(ct);
        return TypedResults.Created($"/api/inspection-plans/{id}/characteristics/{entity.Id}", new IdResponse(entity.Id));
    }

    private static async Task<Results<Ok<IdResponse>, NotFound, ValidationProblem>> UpdateCharAsync(
        int cId, UpdateInspectionPlanCharacteristicRequest request,
        IValidator<UpdateInspectionPlanCharacteristicRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());
        var entity = await db.InspectionPlanCharacteristics.FirstOrDefaultAsync(c => c.Id == cId, ct);
        if (entity is null) return TypedResults.NotFound();
        request.ApplyTo(entity);
        await db.SaveChangesAsync(ct);
        return TypedResults.Ok(new IdResponse(entity.Id));
    }

    private static async Task<Results<NoContent, NotFound>> DeleteCharAsync(
        int cId, ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var entity = await db.InspectionPlanCharacteristics.FirstOrDefaultAsync(c => c.Id == cId, ct);
        if (entity is null) return TypedResults.NotFound();
        db.InspectionPlanCharacteristics.Remove(entity);
        await db.SaveChangesAsync(ct);
        return TypedResults.NoContent();
    }
}
