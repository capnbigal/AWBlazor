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

public static class CountryRegionEndpoints
{
    public static IEndpointRouteBuilder MapCountryRegionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/aw/country-regions")
            .WithTags("CountryRegions")
            .RequireAuthorization("ApiOrCookie");

        group.MapGet("/", ListAsync).WithName("ListCountryRegions").WithSummary("List Person.CountryRegion rows.");
        group.MapGet("/{code}", GetAsync).WithName("GetCountryRegion");
        group.MapPost("/", CreateAsync).WithName("CreateCountryRegion")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));
        group.MapPatch("/{code}", UpdateAsync).WithName("UpdateCountryRegion")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));
        group.MapDelete("/{code}", DeleteAsync).WithName("DeleteCountryRegion")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));
        group.MapGet("/{code}/history", HistoryAsync).WithName("ListCountryRegionHistory");
        return app;
    }

    private static async Task<Ok<PagedResult<CountryRegionDto>>> ListAsync(
        ApplicationDbContext db, [FromQuery] int skip = 0, [FromQuery] int take = 50,
        [FromQuery] string? name = null, CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 1000);
        var query = db.CountryRegions.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(name)) query = query.Where(x => x.Name.Contains(name));
        var total = await query.CountAsync(ct);
        var rows = await query.OrderBy(x => x.CountryRegionCode).Skip(skip).Take(take).Select(x => x.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<CountryRegionDto>(rows, total, skip, take));
    }

    private static async Task<Results<Ok<CountryRegionDto>, NotFound>> GetAsync(string code, ApplicationDbContext db, CancellationToken ct)
    {
        var row = await db.CountryRegions.AsNoTracking().FirstOrDefaultAsync(x => x.CountryRegionCode == code, ct);
        return row is null ? TypedResults.NotFound() : TypedResults.Ok(row.ToDto());
    }

    private static async Task<Results<Created<StringIdResponse>, Conflict<string>, ValidationProblem>> CreateAsync(
        CreateCountryRegionRequest request, IValidator<CreateCountryRegionRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());

        var code = (request.CountryRegionCode ?? string.Empty).Trim();
        if (await db.CountryRegions.AnyAsync(x => x.CountryRegionCode == code, ct))
            return TypedResults.Conflict($"Country/region code '{code}' already exists.");

        var entity = request.ToEntity();
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        db.CountryRegions.Add(entity);
        await db.SaveChangesAsync(ct);
        db.CountryRegionAuditLogs.Add(CountryRegionAuditService.RecordCreate(entity, user.Identity?.Name));
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return TypedResults.Created($"/api/aw/country-regions/{entity.CountryRegionCode}", new StringIdResponse(entity.CountryRegionCode));
    }

    private static async Task<Results<Ok<StringIdResponse>, NotFound, ValidationProblem>> UpdateAsync(
        string code, UpdateCountryRegionRequest request, IValidator<UpdateCountryRegionRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());

        var entity = await db.CountryRegions.FirstOrDefaultAsync(x => x.CountryRegionCode == code, ct);
        if (entity is null) return TypedResults.NotFound();

        var before = CountryRegionAuditService.CaptureSnapshot(entity);
        request.ApplyTo(entity);
        db.CountryRegionAuditLogs.Add(CountryRegionAuditService.RecordUpdate(before, entity, user.Identity?.Name));
        await db.SaveChangesAsync(ct);
        return TypedResults.Ok(new StringIdResponse(entity.CountryRegionCode));
    }

    private static async Task<Results<NoContent, NotFound>> DeleteAsync(
        string code, ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var entity = await db.CountryRegions.FirstOrDefaultAsync(x => x.CountryRegionCode == code, ct);
        if (entity is null) return TypedResults.NotFound();

        db.CountryRegionAuditLogs.Add(CountryRegionAuditService.RecordDelete(entity, user.Identity?.Name));
        db.CountryRegions.Remove(entity);
        await db.SaveChangesAsync(ct);
        return TypedResults.NoContent();
    }

    private static async Task<Ok<List<CountryRegionAuditLogDto>>> HistoryAsync(string code, ApplicationDbContext db, CancellationToken ct)
    {
        var rows = await db.CountryRegionAuditLogs.AsNoTracking()
            .Where(a => a.CountryRegionCode == code)
            .OrderByDescending(a => a.ChangedDate).ThenByDescending(a => a.Id)
            .Select(a => a.ToDto())
            .ToListAsync(ct);
        return TypedResults.Ok(rows);
    }
}
