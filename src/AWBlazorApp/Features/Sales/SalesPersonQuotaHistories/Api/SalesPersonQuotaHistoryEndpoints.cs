using System.Security.Claims;
using AWBlazorApp.Features.Identity.Domain; using AWBlazorApp.Features.Admin.Permissions.Domain;
using AWBlazorApp.Infrastructure.Persistence;
using AWBlazorApp.Shared.Dtos;
using AWBlazorApp.Features.Sales.CountryRegionCurrencies.Dtos; using AWBlazorApp.Features.Sales.CreditCards.Dtos; using AWBlazorApp.Features.Sales.Currencies.Dtos; using AWBlazorApp.Features.Sales.CurrencyRates.Dtos; using AWBlazorApp.Features.Sales.Customers.Dtos; using AWBlazorApp.Features.Sales.PersonCreditCards.Dtos; using AWBlazorApp.Features.Sales.SalesOrderDetails.Dtos; using AWBlazorApp.Features.Sales.SalesOrderHeaders.Dtos; using AWBlazorApp.Features.Sales.SalesOrderHeaderSalesReasons.Dtos; using AWBlazorApp.Features.Sales.SalesPeople.Dtos; using AWBlazorApp.Features.Sales.SalesPersonQuotaHistories.Dtos; using AWBlazorApp.Features.Sales.SalesReasons.Dtos; using AWBlazorApp.Features.Sales.SalesTaxRates.Dtos; using AWBlazorApp.Features.Sales.SalesTerritories.Dtos; using AWBlazorApp.Features.Sales.SalesTerritoryHistories.Dtos; using AWBlazorApp.Features.Sales.ShoppingCartItems.Dtos; using AWBlazorApp.Features.Sales.SpecialOffers.Dtos; using AWBlazorApp.Features.Sales.SpecialOfferProducts.Dtos; using AWBlazorApp.Features.Sales.Stores.Dtos; 
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.Sales.SalesPersonQuotaHistories.Api;

public static class SalesPersonQuotaHistoryEndpoints
{
    public static IEndpointRouteBuilder MapSalesPersonQuotaHistoryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/aw/sales-person-quota-histories")
            .WithTags("SalesPersonQuotaHistories")
            .RequireAuthorization("ApiOrCookie");

        group.MapGet("/", ListAsync).WithName("ListSalesPersonQuotaHistories")
            .WithSummary("List Sales.SalesPersonQuotaHistory rows. Composite PK = (BusinessEntityID, QuotaDate).");
        group.MapGet("/by-key", GetAsync).WithName("GetSalesPersonQuotaHistory");
        group.MapPost("/", CreateAsync).WithName("CreateSalesPersonQuotaHistory")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));
        group.MapPatch("/by-key", UpdateAsync).WithName("UpdateSalesPersonQuotaHistory")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));
        group.MapDelete("/by-key", DeleteAsync).WithName("DeleteSalesPersonQuotaHistory")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));
        group.MapGet("/history", HistoryAsync).WithName("ListSalesPersonQuotaHistoryHistory");
        return app;
    }

    private static async Task<Ok<PagedResult<SalesPersonQuotaHistoryDto>>> ListAsync(
        ApplicationDbContext db, [FromQuery] int skip = 0, [FromQuery] int take = 50,
        [FromQuery] int? businessEntityId = null, CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 1000);
        var query = db.SalesPersonQuotaHistories.AsNoTracking();
        if (businessEntityId.HasValue) query = query.Where(x => x.BusinessEntityId == businessEntityId.Value);
        var total = await query.CountAsync(ct);
        var rows = await query.OrderBy(x => x.BusinessEntityId).ThenByDescending(x => x.QuotaDate)
            .Skip(skip).Take(take).Select(x => x.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<SalesPersonQuotaHistoryDto>(rows, total, skip, take));
    }

    private static async Task<Results<Ok<SalesPersonQuotaHistoryDto>, NotFound>> GetAsync(
        [FromQuery] int businessEntityId, [FromQuery] DateTime quotaDate,
        ApplicationDbContext db, CancellationToken ct)
    {
        var row = await db.SalesPersonQuotaHistories.AsNoTracking()
            .FirstOrDefaultAsync(x => x.BusinessEntityId == businessEntityId && x.QuotaDate == quotaDate, ct);
        return row is null ? TypedResults.NotFound() : TypedResults.Ok(row.ToDto());
    }

    private static async Task<Results<Created<CompositeKeyResponse>, Conflict<string>, ValidationProblem>> CreateAsync(
        CreateSalesPersonQuotaHistoryRequest request, IValidator<CreateSalesPersonQuotaHistoryRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());

        if (await db.SalesPersonQuotaHistories.AnyAsync(x =>
                x.BusinessEntityId == request.BusinessEntityId && x.QuotaDate == request.QuotaDate, ct))
        {
            return TypedResults.Conflict($"Quota row for ({request.BusinessEntityId}, {request.QuotaDate:O}) already exists.");
        }

        var entity = request.ToEntity();
        return TypedResults.Created(
            $"/api/aw/sales-person-quota-histories/by-key?businessEntityId={entity.BusinessEntityId}&quotaDate={entity.QuotaDate:O}",
            new CompositeKeyResponse(new Dictionary<string, object>
            {
                ["businessEntityId"] = entity.BusinessEntityId,
                ["quotaDate"] = entity.QuotaDate,
            }));
    }

    private static async Task<Results<Ok<CompositeKeyResponse>, NotFound, ValidationProblem>> UpdateAsync(
        [FromQuery] int businessEntityId, [FromQuery] DateTime quotaDate,
        UpdateSalesPersonQuotaHistoryRequest request, IValidator<UpdateSalesPersonQuotaHistoryRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());

        var entity = await db.SalesPersonQuotaHistories
            .FirstOrDefaultAsync(x => x.BusinessEntityId == businessEntityId && x.QuotaDate == quotaDate, ct);
        if (entity is null) return TypedResults.NotFound();

        request.ApplyTo(entity);
        await db.SaveChangesAsync(ct);
        return TypedResults.Ok(new CompositeKeyResponse(new Dictionary<string, object>
        {
            ["businessEntityId"] = entity.BusinessEntityId,
            ["quotaDate"] = entity.QuotaDate,
        }));
    }

    private static async Task<Results<NoContent, NotFound>> DeleteAsync(
        [FromQuery] int businessEntityId, [FromQuery] DateTime quotaDate,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var entity = await db.SalesPersonQuotaHistories
            .FirstOrDefaultAsync(x => x.BusinessEntityId == businessEntityId && x.QuotaDate == quotaDate, ct);
        if (entity is null) return TypedResults.NotFound();

        db.SalesPersonQuotaHistories.Remove(entity);
        await db.SaveChangesAsync(ct);
        return TypedResults.NoContent();
    }

    private static async Task<Ok<List<AWBlazorApp.Shared.Audit.AuditLog>>> HistoryAsync(
        ApplicationDbContext db,
        CancellationToken ct = default)
    {
        var rows = await db.AuditLogs.AsNoTracking()
            .Where(a => a.EntityType == "SalesPersonQuotaHistory")
            .OrderByDescending(a => a.ChangedDate).ThenByDescending(a => a.Id)
            .ToListAsync(ct);
        return TypedResults.Ok(rows);
    }

}
