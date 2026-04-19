using System.Security.Claims;
using AWBlazorApp.Features.Identity.Domain; using AWBlazorApp.Features.Admin.Permissions.Domain;
using AWBlazorApp.Infrastructure.Persistence;
using AWBlazorApp.Shared.Dtos;
using AWBlazorApp.Features.Sales.Dtos;
using AWBlazorApp.Features.Sales.Audit;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.Sales.Api;

public static class CurrencyEndpoints
{
    public static IEndpointRouteBuilder MapCurrencyEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/aw/currencies")
            .WithTags("Currencies")
            .RequireAuthorization("ApiOrCookie");

        group.MapGet("/", ListAsync).WithName("ListCurrencies").WithSummary("List Sales.Currency rows.");
        group.MapGet("/{code}", GetAsync).WithName("GetCurrency");
        group.MapPost("/", CreateAsync).WithName("CreateCurrency")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));
        group.MapPatch("/{code}", UpdateAsync).WithName("UpdateCurrency")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));
        group.MapDelete("/{code}", DeleteAsync).WithName("DeleteCurrency")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));
        group.MapGet("/{code}/history", HistoryAsync).WithName("ListCurrencyHistory");
        return app;
    }

    private static async Task<Ok<AWBlazorApp.Shared.Dtos.PagedResult<CurrencyDto>>> ListAsync(
        ApplicationDbContext db, [FromQuery] int skip = 0, [FromQuery] int take = 50,
        [FromQuery] string? name = null, CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 1000);
        var query = db.Currencies.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(name)) query = query.Where(x => x.Name.Contains(name));
        var total = await query.CountAsync(ct);
        var rows = await query.OrderBy(x => x.CurrencyCode).Skip(skip).Take(take).Select(x => x.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new AWBlazorApp.Shared.Dtos.PagedResult<CurrencyDto>(rows, total, skip, take));
    }

    private static async Task<Results<Ok<CurrencyDto>, NotFound>> GetAsync(string code, ApplicationDbContext db, CancellationToken ct)
    {
        var row = await db.Currencies.AsNoTracking().FirstOrDefaultAsync(x => x.CurrencyCode == code, ct);
        return row is null ? TypedResults.NotFound() : TypedResults.Ok(row.ToDto());
    }

    private static async Task<Results<Created<StringIdResponse>, Conflict<string>, ValidationProblem>> CreateAsync(
        CreateCurrencyRequest request, IValidator<CreateCurrencyRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());

        var code = (request.CurrencyCode ?? string.Empty).Trim();
        if (await db.Currencies.AnyAsync(x => x.CurrencyCode == code, ct))
            return TypedResults.Conflict($"Currency code '{code}' already exists.");

        var entity = request.ToEntity();
        await db.AddWithAuditAsync(entity, e => CurrencyAuditService.RecordCreate(e, user.Identity?.Name), ct);
        return TypedResults.Created($"/api/aw/currencies/{entity.CurrencyCode}", new StringIdResponse(entity.CurrencyCode));
    }

    private static async Task<Results<Ok<StringIdResponse>, NotFound, ValidationProblem>> UpdateAsync(
        string code, UpdateCurrencyRequest request, IValidator<UpdateCurrencyRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());

        var entity = await db.Currencies.FirstOrDefaultAsync(x => x.CurrencyCode == code, ct);
        if (entity is null) return TypedResults.NotFound();

        var before = CurrencyAuditService.CaptureSnapshot(entity);
        request.ApplyTo(entity);
        db.CurrencyAuditLogs.Add(CurrencyAuditService.RecordUpdate(before, entity, user.Identity?.Name));
        await db.SaveChangesAsync(ct);
        return TypedResults.Ok(new StringIdResponse(entity.CurrencyCode));
    }

    private static async Task<Results<NoContent, NotFound>> DeleteAsync(
        string code, ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var entity = await db.Currencies.FirstOrDefaultAsync(x => x.CurrencyCode == code, ct);
        if (entity is null) return TypedResults.NotFound();

        db.CurrencyAuditLogs.Add(CurrencyAuditService.RecordDelete(entity, user.Identity?.Name));
        db.Currencies.Remove(entity);
        await db.SaveChangesAsync(ct);
        return TypedResults.NoContent();
    }

    private static async Task<Ok<List<CurrencyAuditLogDto>>> HistoryAsync(string code, ApplicationDbContext db, CancellationToken ct)
    {
        var rows = await db.CurrencyAuditLogs.AsNoTracking()
            .Where(a => a.CurrencyCode == code)
            .OrderByDescending(a => a.ChangedDate).ThenByDescending(a => a.Id)
            .Select(a => a.ToDto())
            .ToListAsync(ct);
        return TypedResults.Ok(rows);
    }
}
