using System.Security.Claims;
using AWBlazorApp.Data;
using AWBlazorApp.Models;
using AWBlazorApp.Models.AdventureWorks;
using AWBlazorApp.Services.AdventureWorksAudit;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Endpoints.AdventureWorks;

public static class CurrencyRateEndpoints
{
    public static IEndpointRouteBuilder MapCurrencyRateEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/aw/currency-rates")
            .WithTags("CurrencyRates")
            .RequireAuthorization("ApiOrCookie");

        group.MapGet("/", ListAsync).WithName("ListCurrencyRates").WithSummary("List Sales.CurrencyRate rows.");
        group.MapGet("/{id:int}", GetAsync).WithName("GetCurrencyRate");
        group.MapPost("/", CreateAsync).WithName("CreateCurrencyRate")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));
        group.MapPatch("/{id:int}", UpdateAsync).WithName("UpdateCurrencyRate")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));
        group.MapDelete("/{id:int}", DeleteAsync).WithName("DeleteCurrencyRate")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));
        group.MapGet("/{id:int}/history", HistoryAsync).WithName("ListCurrencyRateHistory");
        return app;
    }

    private static async Task<Ok<PagedResult<CurrencyRateDto>>> ListAsync(
        ApplicationDbContext db, [FromQuery] int skip = 0, [FromQuery] int take = 50,
        [FromQuery] string? fromCurrencyCode = null, [FromQuery] string? toCurrencyCode = null,
        CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 1000);
        var query = db.CurrencyRates.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(fromCurrencyCode)) query = query.Where(x => x.FromCurrencyCode == fromCurrencyCode);
        if (!string.IsNullOrWhiteSpace(toCurrencyCode)) query = query.Where(x => x.ToCurrencyCode == toCurrencyCode);
        var total = await query.CountAsync(ct);
        var rows = await query.OrderByDescending(x => x.CurrencyRateDate).ThenBy(x => x.Id)
            .Skip(skip).Take(take).Select(x => x.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<CurrencyRateDto>(rows, total, skip, take));
    }

    private static async Task<Results<Ok<CurrencyRateDto>, NotFound>> GetAsync(int id, ApplicationDbContext db, CancellationToken ct)
    {
        var row = await db.CurrencyRates.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        return row is null ? TypedResults.NotFound() : TypedResults.Ok(row.ToDto());
    }

    private static async Task<Results<Created<IdResponse>, ValidationProblem>> CreateAsync(
        CreateCurrencyRateRequest request, IValidator<CreateCurrencyRateRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());

        var entity = request.ToEntity();
        db.CurrencyRates.Add(entity);
        await db.SaveChangesAsync(ct);
        db.CurrencyRateAuditLogs.Add(CurrencyRateAuditService.RecordCreate(entity, user.Identity?.Name));
        await db.SaveChangesAsync(ct);
        return TypedResults.Created($"/api/aw/currency-rates/{entity.Id}", new IdResponse(entity.Id));
    }

    private static async Task<Results<Ok<IdResponse>, NotFound, ValidationProblem>> UpdateAsync(
        int id, UpdateCurrencyRateRequest request, IValidator<UpdateCurrencyRateRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());

        var entity = await db.CurrencyRates.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null) return TypedResults.NotFound();

        var before = CurrencyRateAuditService.CaptureSnapshot(entity);
        request.ApplyTo(entity);
        db.CurrencyRateAuditLogs.Add(CurrencyRateAuditService.RecordUpdate(before, entity, user.Identity?.Name));
        await db.SaveChangesAsync(ct);
        return TypedResults.Ok(new IdResponse(entity.Id));
    }

    private static async Task<Results<NoContent, NotFound>> DeleteAsync(
        int id, ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var entity = await db.CurrencyRates.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null) return TypedResults.NotFound();

        db.CurrencyRateAuditLogs.Add(CurrencyRateAuditService.RecordDelete(entity, user.Identity?.Name));
        db.CurrencyRates.Remove(entity);
        await db.SaveChangesAsync(ct);
        return TypedResults.NoContent();
    }

    private static async Task<Ok<List<CurrencyRateAuditLogDto>>> HistoryAsync(int id, ApplicationDbContext db, CancellationToken ct)
    {
        var rows = await db.CurrencyRateAuditLogs.AsNoTracking()
            .Where(a => a.CurrencyRateId == id)
            .OrderByDescending(a => a.ChangedDate).ThenByDescending(a => a.Id)
            .Select(a => a.ToDto())
            .ToListAsync(ct);
        return TypedResults.Ok(rows);
    }
}
