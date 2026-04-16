using System.Security.Claims;
using AWBlazorApp.Data;
using AWBlazorApp.Infrastructure.Persistence;
using AWBlazorApp.Shared.Models;
using AWBlazorApp.Features.Sales.Models;
using AWBlazorApp.Features.Sales.Audit;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.Sales.Endpoints;

public static class SalesTaxRateEndpoints
{
    public static IEndpointRouteBuilder MapSalesTaxRateEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/aw/sales-tax-rates")
            .WithTags("SalesTaxRates")
            .RequireAuthorization("ApiOrCookie");

        group.MapGet("/", ListAsync).WithName("ListSalesTaxRates").WithSummary("List Sales.SalesTaxRate rows.");
        group.MapGet("/{id:int}", GetAsync).WithName("GetSalesTaxRate");
        group.MapPost("/", CreateAsync).WithName("CreateSalesTaxRate")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));
        group.MapPatch("/{id:int}", UpdateAsync).WithName("UpdateSalesTaxRate")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));
        group.MapDelete("/{id:int}", DeleteAsync).WithName("DeleteSalesTaxRate")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));
        group.MapGet("/{id:int}/history", HistoryAsync).WithName("ListSalesTaxRateHistory");
        return app;
    }

    private static async Task<Ok<PagedResult<SalesTaxRateDto>>> ListAsync(
        ApplicationDbContext db, [FromQuery] int skip = 0, [FromQuery] int take = 50,
        [FromQuery] string? name = null, [FromQuery] int? stateProvinceId = null, CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 1000);
        var query = db.SalesTaxRates.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(name)) query = query.Where(x => x.Name.Contains(name));
        if (stateProvinceId.HasValue) query = query.Where(x => x.StateProvinceId == stateProvinceId.Value);
        var total = await query.CountAsync(ct);
        var rows = await query.OrderBy(x => x.Id).Skip(skip).Take(take).Select(x => x.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<SalesTaxRateDto>(rows, total, skip, take));
    }

    private static async Task<Results<Ok<SalesTaxRateDto>, NotFound>> GetAsync(int id, ApplicationDbContext db, CancellationToken ct)
    {
        var row = await db.SalesTaxRates.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        return row is null ? TypedResults.NotFound() : TypedResults.Ok(row.ToDto());
    }

    private static async Task<Results<Created<IdResponse>, ValidationProblem>> CreateAsync(
        CreateSalesTaxRateRequest request, IValidator<CreateSalesTaxRateRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());

        var entity = request.ToEntity();
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        db.SalesTaxRates.Add(entity);
        await db.SaveChangesAsync(ct);
        db.SalesTaxRateAuditLogs.Add(SalesTaxRateAuditService.RecordCreate(entity, user.Identity?.Name));
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return TypedResults.Created($"/api/aw/sales-tax-rates/{entity.Id}", new IdResponse(entity.Id));
    }

    private static async Task<Results<Ok<IdResponse>, NotFound, ValidationProblem>> UpdateAsync(
        int id, UpdateSalesTaxRateRequest request, IValidator<UpdateSalesTaxRateRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());

        var entity = await db.SalesTaxRates.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null) return TypedResults.NotFound();

        var before = SalesTaxRateAuditService.CaptureSnapshot(entity);
        request.ApplyTo(entity);
        db.SalesTaxRateAuditLogs.Add(SalesTaxRateAuditService.RecordUpdate(before, entity, user.Identity?.Name));
        await db.SaveChangesAsync(ct);
        return TypedResults.Ok(new IdResponse(entity.Id));
    }

    private static async Task<Results<NoContent, NotFound>> DeleteAsync(
        int id, ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var entity = await db.SalesTaxRates.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null) return TypedResults.NotFound();

        db.SalesTaxRateAuditLogs.Add(SalesTaxRateAuditService.RecordDelete(entity, user.Identity?.Name));
        db.SalesTaxRates.Remove(entity);
        await db.SaveChangesAsync(ct);
        return TypedResults.NoContent();
    }

    private static async Task<Ok<List<SalesTaxRateAuditLogDto>>> HistoryAsync(int id, ApplicationDbContext db, CancellationToken ct)
    {
        var rows = await db.SalesTaxRateAuditLogs.AsNoTracking()
            .Where(a => a.SalesTaxRateId == id)
            .OrderByDescending(a => a.ChangedDate).ThenByDescending(a => a.Id)
            .Select(a => a.ToDto())
            .ToListAsync(ct);
        return TypedResults.Ok(rows);
    }
}
