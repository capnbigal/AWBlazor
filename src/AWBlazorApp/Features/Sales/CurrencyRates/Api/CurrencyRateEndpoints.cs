using AWBlazorApp.Features.Identity.Domain; using AWBlazorApp.Features.Admin.Permissions.Domain;
using AWBlazorApp.Infrastructure.Persistence;
using AWBlazorApp.Shared.Api;
using AWBlazorApp.Shared.Dtos;
using AWBlazorApp.Features.Sales.CountryRegionCurrencies.Application.Services; using AWBlazorApp.Features.Sales.CreditCards.Application.Services; using AWBlazorApp.Features.Sales.Currencies.Application.Services; using AWBlazorApp.Features.Sales.CurrencyRates.Application.Services; using AWBlazorApp.Features.Sales.Customers.Application.Services; using AWBlazorApp.Features.Sales.PersonCreditCards.Application.Services; using AWBlazorApp.Features.Sales.SalesOrderDetails.Application.Services; using AWBlazorApp.Features.Sales.SalesOrderHeaders.Application.Services; using AWBlazorApp.Features.Sales.SalesOrderHeaderSalesReasons.Application.Services; using AWBlazorApp.Features.Sales.SalesPeople.Application.Services; using AWBlazorApp.Features.Sales.SalesPersonQuotaHistories.Application.Services; using AWBlazorApp.Features.Sales.SalesReasons.Application.Services; using AWBlazorApp.Features.Sales.SalesTaxRates.Application.Services; using AWBlazorApp.Features.Sales.SalesTerritories.Application.Services; using AWBlazorApp.Features.Sales.SalesTerritoryHistories.Application.Services; using AWBlazorApp.Features.Sales.ShoppingCartItems.Application.Services; using AWBlazorApp.Features.Sales.SpecialOffers.Application.Services; using AWBlazorApp.Features.Sales.SpecialOfferProducts.Application.Services; using AWBlazorApp.Features.Sales.Stores.Application.Services; 
using AWBlazorApp.Features.Sales.CountryRegionCurrencies.Domain; using AWBlazorApp.Features.Sales.CreditCards.Domain; using AWBlazorApp.Features.Sales.Currencies.Domain; using AWBlazorApp.Features.Sales.CurrencyRates.Domain; using AWBlazorApp.Features.Sales.Customers.Domain; using AWBlazorApp.Features.Sales.PersonCreditCards.Domain; using AWBlazorApp.Features.Sales.SalesOrderDetails.Domain; using AWBlazorApp.Features.Sales.SalesOrderHeaders.Domain; using AWBlazorApp.Features.Sales.SalesOrderHeaderSalesReasons.Domain; using AWBlazorApp.Features.Sales.SalesPeople.Domain; using AWBlazorApp.Features.Sales.SalesPersonQuotaHistories.Domain; using AWBlazorApp.Features.Sales.SalesReasons.Domain; using AWBlazorApp.Features.Sales.SalesTaxRates.Domain; using AWBlazorApp.Features.Sales.SalesTerritories.Domain; using AWBlazorApp.Features.Sales.SalesTerritoryHistories.Domain; using AWBlazorApp.Features.Sales.ShoppingCartItems.Domain; using AWBlazorApp.Features.Sales.SpecialOffers.Domain; using AWBlazorApp.Features.Sales.SpecialOfferProducts.Domain; using AWBlazorApp.Features.Sales.Stores.Domain; 
using AWBlazorApp.Features.Sales.CountryRegionCurrencies.Dtos; using AWBlazorApp.Features.Sales.CreditCards.Dtos; using AWBlazorApp.Features.Sales.Currencies.Dtos; using AWBlazorApp.Features.Sales.CurrencyRates.Dtos; using AWBlazorApp.Features.Sales.Customers.Dtos; using AWBlazorApp.Features.Sales.PersonCreditCards.Dtos; using AWBlazorApp.Features.Sales.SalesOrderDetails.Dtos; using AWBlazorApp.Features.Sales.SalesOrderHeaders.Dtos; using AWBlazorApp.Features.Sales.SalesOrderHeaderSalesReasons.Dtos; using AWBlazorApp.Features.Sales.SalesPeople.Dtos; using AWBlazorApp.Features.Sales.SalesPersonQuotaHistories.Dtos; using AWBlazorApp.Features.Sales.SalesReasons.Dtos; using AWBlazorApp.Features.Sales.SalesTaxRates.Dtos; using AWBlazorApp.Features.Sales.SalesTerritories.Dtos; using AWBlazorApp.Features.Sales.SalesTerritoryHistories.Dtos; using AWBlazorApp.Features.Sales.ShoppingCartItems.Dtos; using AWBlazorApp.Features.Sales.SpecialOffers.Dtos; using AWBlazorApp.Features.Sales.SpecialOfferProducts.Dtos; using AWBlazorApp.Features.Sales.Stores.Dtos; 
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.Sales.CurrencyRates.Api;

public static class CurrencyRateEndpoints
{
    public static IEndpointRouteBuilder MapCurrencyRateEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/aw/currency-rates")
            .WithTags("CurrencyRates")
            .RequireAuthorization("ApiOrCookie");

        group.MapGet("/", ListAsync).WithName("ListCurrencyRates").WithSummary("List Sales.CurrencyRate rows.");

        group.MapIntIdCrud<CurrencyRate, CurrencyRateDto, CreateCurrencyRateRequest, UpdateCurrencyRateRequest, CurrencyRateAuditLog, CurrencyRateAuditLogDto, CurrencyRateAuditService.Snapshot, int>(
            entityName: "CurrencyRate",
            routePrefix: "/api/aw/currency-rates",
            entitySet: db => db.CurrencyRates,
            auditSet: db => db.CurrencyRateAuditLogs,
            idSelector: e => e.Id,
            auditIdSelector: a => a.CurrencyRateId,
            auditChangedDateSelector: a => a.ChangedDate,
            auditPrimaryKeySelector: a => a.Id,
            getId: e => e.Id,
            toDto: e => e.ToDto(),
            toEntity: r => r.ToEntity(),
            applyUpdate: (r, e) => r.ApplyTo(e),
            captureSnapshot: CurrencyRateAuditService.CaptureSnapshot,
            recordCreate: CurrencyRateAuditService.RecordCreate,
            recordUpdate: CurrencyRateAuditService.RecordUpdate,
            recordDelete: CurrencyRateAuditService.RecordDelete,
            auditToDto: a => a.ToDto());

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
}