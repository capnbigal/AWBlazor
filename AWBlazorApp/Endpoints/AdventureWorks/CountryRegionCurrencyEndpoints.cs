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

public static class CountryRegionCurrencyEndpoints
{
    public static IEndpointRouteBuilder MapCountryRegionCurrencyEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/aw/country-region-currencies")
            .WithTags("CountryRegionCurrencies")
            .RequireAuthorization("ApiOrCookie");

        group.MapGet("/", ListAsync).WithName("ListCountryRegionCurrencies")
            .WithSummary("List Sales.CountryRegionCurrency rows. Composite PK = (CountryRegionCode, CurrencyCode).");
        group.MapGet("/by-key", GetAsync).WithName("GetCountryRegionCurrency");
        group.MapPost("/", CreateAsync).WithName("CreateCountryRegionCurrency")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));
        group.MapPatch("/by-key", UpdateAsync).WithName("UpdateCountryRegionCurrency")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));
        group.MapDelete("/by-key", DeleteAsync).WithName("DeleteCountryRegionCurrency")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));
        group.MapGet("/history", HistoryAsync).WithName("ListCountryRegionCurrencyHistory");
        return app;
    }

    private static async Task<Ok<PagedResult<CountryRegionCurrencyDto>>> ListAsync(
        ApplicationDbContext db, [FromQuery] int skip = 0, [FromQuery] int take = 50,
        [FromQuery] string? countryRegionCode = null, CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 1000);
        var query = db.CountryRegionCurrencies.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(countryRegionCode))
            query = query.Where(x => x.CountryRegionCode == countryRegionCode);
        var total = await query.CountAsync(ct);
        var rows = await query.OrderBy(x => x.CountryRegionCode).ThenBy(x => x.CurrencyCode)
            .Skip(skip).Take(take).Select(x => x.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<CountryRegionCurrencyDto>(rows, total, skip, take));
    }

    private static async Task<Results<Ok<CountryRegionCurrencyDto>, NotFound>> GetAsync(
        [FromQuery] string countryRegionCode, [FromQuery] string currencyCode,
        ApplicationDbContext db, CancellationToken ct)
    {
        var row = await db.CountryRegionCurrencies.AsNoTracking()
            .FirstOrDefaultAsync(x => x.CountryRegionCode == countryRegionCode && x.CurrencyCode == currencyCode, ct);
        return row is null ? TypedResults.NotFound() : TypedResults.Ok(row.ToDto());
    }

    private static async Task<Results<Created<CompositeKeyResponse>, Conflict<string>, ValidationProblem>> CreateAsync(
        CreateCountryRegionCurrencyRequest request, IValidator<CreateCountryRegionCurrencyRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());

        var crc = (request.CountryRegionCode ?? string.Empty).Trim();
        var cur = (request.CurrencyCode ?? string.Empty).Trim();
        if (await db.CountryRegionCurrencies.AnyAsync(x =>
                x.CountryRegionCode == crc && x.CurrencyCode == cur, ct))
        {
            return TypedResults.Conflict($"Junction row ({crc}, {cur}) already exists.");
        }

        var entity = request.ToEntity();
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        db.CountryRegionCurrencies.Add(entity);
        await db.SaveChangesAsync(ct);
        db.CountryRegionCurrencyAuditLogs.Add(
            CountryRegionCurrencyAuditService.RecordCreate(entity, user.Identity?.Name));
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return TypedResults.Created(
            $"/api/aw/country-region-currencies/by-key?countryRegionCode={entity.CountryRegionCode}&currencyCode={entity.CurrencyCode}",
            new CompositeKeyResponse(new Dictionary<string, object>
            {
                ["countryRegionCode"] = entity.CountryRegionCode,
                ["currencyCode"] = entity.CurrencyCode,
            }));
    }

    private static async Task<Results<Ok<CompositeKeyResponse>, NotFound, ValidationProblem>> UpdateAsync(
        [FromQuery] string countryRegionCode, [FromQuery] string currencyCode,
        UpdateCountryRegionCurrencyRequest request, IValidator<UpdateCountryRegionCurrencyRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());

        var entity = await db.CountryRegionCurrencies
            .FirstOrDefaultAsync(x => x.CountryRegionCode == countryRegionCode && x.CurrencyCode == currencyCode, ct);
        if (entity is null) return TypedResults.NotFound();

        request.ApplyTo(entity);
        db.CountryRegionCurrencyAuditLogs.Add(
            CountryRegionCurrencyAuditService.RecordUpdate(entity, user.Identity?.Name));
        await db.SaveChangesAsync(ct);
        return TypedResults.Ok(new CompositeKeyResponse(new Dictionary<string, object>
        {
            ["countryRegionCode"] = entity.CountryRegionCode,
            ["currencyCode"] = entity.CurrencyCode,
        }));
    }

    private static async Task<Results<NoContent, NotFound>> DeleteAsync(
        [FromQuery] string countryRegionCode, [FromQuery] string currencyCode,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var entity = await db.CountryRegionCurrencies
            .FirstOrDefaultAsync(x => x.CountryRegionCode == countryRegionCode && x.CurrencyCode == currencyCode, ct);
        if (entity is null) return TypedResults.NotFound();

        db.CountryRegionCurrencyAuditLogs.Add(
            CountryRegionCurrencyAuditService.RecordDelete(entity, user.Identity?.Name));
        db.CountryRegionCurrencies.Remove(entity);
        await db.SaveChangesAsync(ct);
        return TypedResults.NoContent();
    }

    private static async Task<Ok<List<CountryRegionCurrencyAuditLogDto>>> HistoryAsync(
        ApplicationDbContext db,
        [FromQuery] string? countryRegionCode = null,
        [FromQuery] string? currencyCode = null,
        CancellationToken ct = default)
    {
        var query = db.CountryRegionCurrencyAuditLogs.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(countryRegionCode))
            query = query.Where(a => a.CountryRegionCode == countryRegionCode);
        if (!string.IsNullOrWhiteSpace(currencyCode))
            query = query.Where(a => a.CurrencyCode == currencyCode);

        var rows = await query
            .OrderByDescending(a => a.ChangedDate).ThenByDescending(a => a.Id)
            .Select(a => a.ToDto())
            .ToListAsync(ct);
        return TypedResults.Ok(rows);
    }
}
