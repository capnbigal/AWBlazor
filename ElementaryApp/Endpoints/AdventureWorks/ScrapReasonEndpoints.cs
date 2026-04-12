using System.Security.Claims;
using ElementaryApp.Data;
using ElementaryApp.Models;
using ElementaryApp.Models.AdventureWorks;
using ElementaryApp.Services.AdventureWorksAudit;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ElementaryApp.Endpoints.AdventureWorks;

public static class ScrapReasonEndpoints
{
    public static IEndpointRouteBuilder MapScrapReasonEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/aw/scrap-reasons")
            .WithTags("ScrapReasons")
            .RequireAuthorization("ApiOrCookie");

        group.MapGet("/", ListAsync).WithName("ListScrapReasons").WithSummary("List Production.ScrapReason rows.");
        group.MapGet("/{id:int}", GetAsync).WithName("GetScrapReason");
        group.MapPost("/", CreateAsync).WithName("CreateScrapReason")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));
        group.MapPatch("/{id:int}", UpdateAsync).WithName("UpdateScrapReason")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));
        group.MapDelete("/{id:int}", DeleteAsync).WithName("DeleteScrapReason")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));
        group.MapGet("/{id:int}/history", HistoryAsync).WithName("ListScrapReasonHistory");
        return app;
    }

    private static async Task<Ok<PagedResult<ScrapReasonDto>>> ListAsync(
        ApplicationDbContext db, [FromQuery] int skip = 0, [FromQuery] int take = 50,
        [FromQuery] string? name = null, CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 1000);
        var query = db.ScrapReasons.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(name)) query = query.Where(x => x.Name.Contains(name));
        var total = await query.CountAsync(ct);
        var rows = await query.OrderBy(x => x.Id).Skip(skip).Take(take).Select(x => x.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<ScrapReasonDto>(rows, total, skip, take));
    }

    private static async Task<Results<Ok<ScrapReasonDto>, NotFound>> GetAsync(int id, ApplicationDbContext db, CancellationToken ct)
    {
        var row = await db.ScrapReasons.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        return row is null ? TypedResults.NotFound() : TypedResults.Ok(row.ToDto());
    }

    private static async Task<Results<Created<IdResponse>, ValidationProblem>> CreateAsync(
        CreateScrapReasonRequest request, IValidator<CreateScrapReasonRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());

        var entity = request.ToEntity();
        db.ScrapReasons.Add(entity);
        await db.SaveChangesAsync(ct);
        db.ScrapReasonAuditLogs.Add(ScrapReasonAuditService.RecordCreate(entity, user.Identity?.Name));
        await db.SaveChangesAsync(ct);
        return TypedResults.Created($"/api/aw/scrap-reasons/{entity.Id}", new IdResponse(entity.Id));
    }

    private static async Task<Results<Ok<IdResponse>, NotFound, ValidationProblem>> UpdateAsync(
        int id, UpdateScrapReasonRequest request, IValidator<UpdateScrapReasonRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());

        var key = (short)id;
        var entity = await db.ScrapReasons.FirstOrDefaultAsync(x => x.Id == key, ct);
        if (entity is null) return TypedResults.NotFound();

        var before = ScrapReasonAuditService.CaptureSnapshot(entity);
        request.ApplyTo(entity);
        db.ScrapReasonAuditLogs.Add(ScrapReasonAuditService.RecordUpdate(before, entity, user.Identity?.Name));
        await db.SaveChangesAsync(ct);
        return TypedResults.Ok(new IdResponse(entity.Id));
    }

    private static async Task<Results<NoContent, NotFound>> DeleteAsync(
        int id, ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var key = (short)id;
        var entity = await db.ScrapReasons.FirstOrDefaultAsync(x => x.Id == key, ct);
        if (entity is null) return TypedResults.NotFound();

        db.ScrapReasonAuditLogs.Add(ScrapReasonAuditService.RecordDelete(entity, user.Identity?.Name));
        db.ScrapReasons.Remove(entity);
        await db.SaveChangesAsync(ct);
        return TypedResults.NoContent();
    }

    private static async Task<Ok<List<ScrapReasonAuditLogDto>>> HistoryAsync(int id, ApplicationDbContext db, CancellationToken ct)
    {
        var key = (short)id;
        var rows = await db.ScrapReasonAuditLogs.AsNoTracking()
            .Where(a => a.ScrapReasonId == key)
            .OrderByDescending(a => a.ChangedDate).ThenByDescending(a => a.Id)
            .Select(a => a.ToDto())
            .ToListAsync(ct);
        return TypedResults.Ok(rows);
    }
}
