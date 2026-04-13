using System.Security.Claims;
using AWBlazorApp.Data;
using AWBlazorApp.Models.AdventureWorks;
using AWBlazorApp.Services.AdventureWorksAudit;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Endpoints.AdventureWorks;

public static class CultureEndpoints
{
    public static IEndpointRouteBuilder MapCultureEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/aw/cultures")
            .WithTags("Cultures")
            .RequireAuthorization("ApiOrCookie");

        group.MapGet("/", ListAsync).WithName("ListCultures").WithSummary("List Production.Culture rows.");
        group.MapGet("/{id}", GetAsync).WithName("GetCulture");
        group.MapPost("/", CreateAsync).WithName("CreateCulture")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));
        group.MapPatch("/{id}", UpdateAsync).WithName("UpdateCulture")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));
        group.MapDelete("/{id}", DeleteAsync).WithName("DeleteCulture")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));
        group.MapGet("/{id}/history", HistoryAsync).WithName("ListCultureHistory");
        return app;
    }

    private static async Task<Ok<Models.PagedResult<CultureDto>>> ListAsync(
        ApplicationDbContext db, [FromQuery] int skip = 0, [FromQuery] int take = 50,
        [FromQuery] string? name = null, CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 1000);
        var query = db.Cultures.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(name)) query = query.Where(x => x.Name.Contains(name));
        var total = await query.CountAsync(ct);
        var rows = await query.OrderBy(x => x.CultureId).Skip(skip).Take(take).Select(x => x.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new Models.PagedResult<CultureDto>(rows, total, skip, take));
    }

    private static async Task<Results<Ok<CultureDto>, NotFound>> GetAsync(string id, ApplicationDbContext db, CancellationToken ct)
    {
        var row = await db.Cultures.AsNoTracking().FirstOrDefaultAsync(x => x.CultureId == id, ct);
        return row is null ? TypedResults.NotFound() : TypedResults.Ok(row.ToDto());
    }

    private static async Task<Results<Created<StringIdResponse>, Conflict<string>, ValidationProblem>> CreateAsync(
        CreateCultureRequest request, IValidator<CreateCultureRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());

        var id = (request.CultureId ?? string.Empty).Trim();
        if (await db.Cultures.AnyAsync(x => x.CultureId == id, ct))
            return TypedResults.Conflict($"Culture ID '{id}' already exists.");

        var entity = request.ToEntity();
        db.Cultures.Add(entity);
        await db.SaveChangesAsync(ct);
        db.CultureAuditLogs.Add(CultureAuditService.RecordCreate(entity, user.Identity?.Name));
        await db.SaveChangesAsync(ct);
        return TypedResults.Created($"/api/aw/cultures/{entity.CultureId}", new StringIdResponse(entity.CultureId));
    }

    private static async Task<Results<Ok<StringIdResponse>, NotFound, ValidationProblem>> UpdateAsync(
        string id, UpdateCultureRequest request, IValidator<UpdateCultureRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());

        var entity = await db.Cultures.FirstOrDefaultAsync(x => x.CultureId == id, ct);
        if (entity is null) return TypedResults.NotFound();

        var before = CultureAuditService.CaptureSnapshot(entity);
        request.ApplyTo(entity);
        db.CultureAuditLogs.Add(CultureAuditService.RecordUpdate(before, entity, user.Identity?.Name));
        await db.SaveChangesAsync(ct);
        return TypedResults.Ok(new StringIdResponse(entity.CultureId));
    }

    private static async Task<Results<NoContent, NotFound>> DeleteAsync(
        string id, ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var entity = await db.Cultures.FirstOrDefaultAsync(x => x.CultureId == id, ct);
        if (entity is null) return TypedResults.NotFound();

        db.CultureAuditLogs.Add(CultureAuditService.RecordDelete(entity, user.Identity?.Name));
        db.Cultures.Remove(entity);
        await db.SaveChangesAsync(ct);
        return TypedResults.NoContent();
    }

    private static async Task<Ok<List<CultureAuditLogDto>>> HistoryAsync(string id, ApplicationDbContext db, CancellationToken ct)
    {
        var rows = await db.CultureAuditLogs.AsNoTracking()
            .Where(a => a.CultureId == id)
            .OrderByDescending(a => a.ChangedDate).ThenByDescending(a => a.Id)
            .Select(a => a.ToDto())
            .ToListAsync(ct);
        return TypedResults.Ok(rows);
    }
}
