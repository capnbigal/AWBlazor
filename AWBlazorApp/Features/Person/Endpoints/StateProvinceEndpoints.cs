using System.Security.Claims;
using AWBlazorApp.Data;
using AWBlazorApp.Infrastructure.Persistence;
using AWBlazorApp.Shared.Models;
using AWBlazorApp.Features.Person.Models;
using AWBlazorApp.Features.Person.Audit;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.Person.Endpoints;

public static class StateProvinceEndpoints
{
    public static IEndpointRouteBuilder MapStateProvinceEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/aw/state-provinces")
            .WithTags("StateProvinces")
            .RequireAuthorization("ApiOrCookie");

        group.MapGet("/", ListAsync).WithName("ListStateProvinces").WithSummary("List Person.StateProvince rows.");
        group.MapGet("/{id:int}", GetAsync).WithName("GetStateProvince");
        group.MapPost("/", CreateAsync).WithName("CreateStateProvince")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));
        group.MapPatch("/{id:int}", UpdateAsync).WithName("UpdateStateProvince")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));
        group.MapDelete("/{id:int}", DeleteAsync).WithName("DeleteStateProvince")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));
        group.MapGet("/{id:int}/history", HistoryAsync).WithName("ListStateProvinceHistory");
        return app;
    }

    private static async Task<Ok<PagedResult<StateProvinceDto>>> ListAsync(
        ApplicationDbContext db, [FromQuery] int skip = 0, [FromQuery] int take = 50,
        [FromQuery] string? name = null, [FromQuery] string? countryRegionCode = null, CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 1000);
        var query = db.StateProvinces.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(name)) query = query.Where(x => x.Name.Contains(name));
        if (!string.IsNullOrWhiteSpace(countryRegionCode)) query = query.Where(x => x.CountryRegionCode == countryRegionCode);
        var total = await query.CountAsync(ct);
        var rows = await query.OrderBy(x => x.Id).Skip(skip).Take(take).Select(x => x.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<StateProvinceDto>(rows, total, skip, take));
    }

    private static async Task<Results<Ok<StateProvinceDto>, NotFound>> GetAsync(int id, ApplicationDbContext db, CancellationToken ct)
    {
        var row = await db.StateProvinces.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        return row is null ? TypedResults.NotFound() : TypedResults.Ok(row.ToDto());
    }

    private static async Task<Results<Created<IdResponse>, ValidationProblem>> CreateAsync(
        CreateStateProvinceRequest request, IValidator<CreateStateProvinceRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());

        var entity = request.ToEntity();
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        db.StateProvinces.Add(entity);
        await db.SaveChangesAsync(ct);
        db.StateProvinceAuditLogs.Add(StateProvinceAuditService.RecordCreate(entity, user.Identity?.Name));
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return TypedResults.Created($"/api/aw/state-provinces/{entity.Id}", new IdResponse(entity.Id));
    }

    private static async Task<Results<Ok<IdResponse>, NotFound, ValidationProblem>> UpdateAsync(
        int id, UpdateStateProvinceRequest request, IValidator<UpdateStateProvinceRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());

        var entity = await db.StateProvinces.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null) return TypedResults.NotFound();

        var before = StateProvinceAuditService.CaptureSnapshot(entity);
        request.ApplyTo(entity);
        db.StateProvinceAuditLogs.Add(StateProvinceAuditService.RecordUpdate(before, entity, user.Identity?.Name));
        await db.SaveChangesAsync(ct);
        return TypedResults.Ok(new IdResponse(entity.Id));
    }

    private static async Task<Results<NoContent, NotFound>> DeleteAsync(
        int id, ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var entity = await db.StateProvinces.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null) return TypedResults.NotFound();

        db.StateProvinceAuditLogs.Add(StateProvinceAuditService.RecordDelete(entity, user.Identity?.Name));
        db.StateProvinces.Remove(entity);
        await db.SaveChangesAsync(ct);
        return TypedResults.NoContent();
    }

    private static async Task<Ok<List<StateProvinceAuditLogDto>>> HistoryAsync(int id, ApplicationDbContext db, CancellationToken ct)
    {
        var rows = await db.StateProvinceAuditLogs.AsNoTracking()
            .Where(a => a.StateProvinceId == id)
            .OrderByDescending(a => a.ChangedDate).ThenByDescending(a => a.Id)
            .Select(a => a.ToDto())
            .ToListAsync(ct);
        return TypedResults.Ok(rows);
    }
}
