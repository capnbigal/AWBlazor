using System.Security.Claims;
using AWBlazorApp.Data;
using AWBlazorApp.Infrastructure.Persistence;
using AWBlazorApp.Shared.Models;
using AWBlazorApp.Features.Production.Models;
using AWBlazorApp.Features.Production.Audit;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.Production.Endpoints;

public static class UnitMeasureEndpoints
{
    public static IEndpointRouteBuilder MapUnitMeasureEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/aw/unit-measures")
            .WithTags("UnitMeasures")
            .RequireAuthorization("ApiOrCookie");

        group.MapGet("/", ListAsync).WithName("ListUnitMeasures").WithSummary("List Production.UnitMeasure rows.");
        group.MapGet("/{code}", GetAsync).WithName("GetUnitMeasure");
        group.MapPost("/", CreateAsync).WithName("CreateUnitMeasure")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));
        group.MapPatch("/{code}", UpdateAsync).WithName("UpdateUnitMeasure")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));
        group.MapDelete("/{code}", DeleteAsync).WithName("DeleteUnitMeasure")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));
        group.MapGet("/{code}/history", HistoryAsync).WithName("ListUnitMeasureHistory");
        return app;
    }

    private static async Task<Ok<AWBlazorApp.Shared.Models.PagedResult<UnitMeasureDto>>> ListAsync(
        ApplicationDbContext db, [FromQuery] int skip = 0, [FromQuery] int take = 50,
        [FromQuery] string? name = null, CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 1000);
        var query = db.UnitMeasures.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(name)) query = query.Where(x => x.Name.Contains(name));
        var total = await query.CountAsync(ct);
        var rows = await query.OrderBy(x => x.UnitMeasureCode).Skip(skip).Take(take).Select(x => x.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new AWBlazorApp.Shared.Models.PagedResult<UnitMeasureDto>(rows, total, skip, take));
    }

    private static async Task<Results<Ok<UnitMeasureDto>, NotFound>> GetAsync(string code, ApplicationDbContext db, CancellationToken ct)
    {
        var row = await db.UnitMeasures.AsNoTracking().FirstOrDefaultAsync(x => x.UnitMeasureCode == code, ct);
        return row is null ? TypedResults.NotFound() : TypedResults.Ok(row.ToDto());
    }

    private static async Task<Results<Created<StringIdResponse>, Conflict<string>, ValidationProblem>> CreateAsync(
        CreateUnitMeasureRequest request, IValidator<CreateUnitMeasureRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());

        var code = (request.UnitMeasureCode ?? string.Empty).Trim();
        if (await db.UnitMeasures.AnyAsync(x => x.UnitMeasureCode == code, ct))
            return TypedResults.Conflict($"Unit measure code '{code}' already exists.");

        var entity = request.ToEntity();
        await db.AddWithAuditAsync(entity, e => UnitMeasureAuditService.RecordCreate(e, user.Identity?.Name), ct);
        return TypedResults.Created($"/api/aw/unit-measures/{entity.UnitMeasureCode}", new StringIdResponse(entity.UnitMeasureCode));
    }

    private static async Task<Results<Ok<StringIdResponse>, NotFound, ValidationProblem>> UpdateAsync(
        string code, UpdateUnitMeasureRequest request, IValidator<UpdateUnitMeasureRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());

        var entity = await db.UnitMeasures.FirstOrDefaultAsync(x => x.UnitMeasureCode == code, ct);
        if (entity is null) return TypedResults.NotFound();

        var before = UnitMeasureAuditService.CaptureSnapshot(entity);
        request.ApplyTo(entity);
        db.UnitMeasureAuditLogs.Add(UnitMeasureAuditService.RecordUpdate(before, entity, user.Identity?.Name));
        await db.SaveChangesAsync(ct);
        return TypedResults.Ok(new StringIdResponse(entity.UnitMeasureCode));
    }

    private static async Task<Results<NoContent, NotFound>> DeleteAsync(
        string code, ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var entity = await db.UnitMeasures.FirstOrDefaultAsync(x => x.UnitMeasureCode == code, ct);
        if (entity is null) return TypedResults.NotFound();

        db.UnitMeasureAuditLogs.Add(UnitMeasureAuditService.RecordDelete(entity, user.Identity?.Name));
        db.UnitMeasures.Remove(entity);
        await db.SaveChangesAsync(ct);
        return TypedResults.NoContent();
    }

    private static async Task<Ok<List<UnitMeasureAuditLogDto>>> HistoryAsync(string code, ApplicationDbContext db, CancellationToken ct)
    {
        var rows = await db.UnitMeasureAuditLogs.AsNoTracking()
            .Where(a => a.UnitMeasureCode == code)
            .OrderByDescending(a => a.ChangedDate).ThenByDescending(a => a.Id)
            .Select(a => a.ToDto())
            .ToListAsync(ct);
        return TypedResults.Ok(rows);
    }
}
