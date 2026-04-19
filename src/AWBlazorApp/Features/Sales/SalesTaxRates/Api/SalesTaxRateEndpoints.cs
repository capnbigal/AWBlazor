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

namespace AWBlazorApp.Features.Sales.SalesTaxRates.Api;

public static class SalesTaxRateEndpoints
{
    public static IEndpointRouteBuilder MapSalesTaxRateEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/aw/sales-tax-rates")
            .WithTags("SalesTaxRates")
            .RequireAuthorization("ApiOrCookie");

        group.MapGet("/", ListAsync).WithName("ListSalesTaxRates").WithSummary("List Sales.SalesTaxRate rows.");

        group.MapIntIdCrud<SalesTaxRate, SalesTaxRateDto, CreateSalesTaxRateRequest, UpdateSalesTaxRateRequest, SalesTaxRateAuditLog, SalesTaxRateAuditLogDto, SalesTaxRateAuditService.Snapshot, int>(
            entityName: "SalesTaxRate",
            routePrefix: "/api/aw/sales-tax-rates",
            entitySet: db => db.SalesTaxRates,
            auditSet: db => db.SalesTaxRateAuditLogs,
            idSelector: e => e.Id,
            auditIdSelector: a => a.SalesTaxRateId,
            auditChangedDateSelector: a => a.ChangedDate,
            auditPrimaryKeySelector: a => a.Id,
            getId: e => e.Id,
            toDto: e => e.ToDto(),
            toEntity: r => r.ToEntity(),
            applyUpdate: (r, e) => r.ApplyTo(e),
            captureSnapshot: SalesTaxRateAuditService.CaptureSnapshot,
            recordCreate: SalesTaxRateAuditService.RecordCreate,
            recordUpdate: SalesTaxRateAuditService.RecordUpdate,
            recordDelete: SalesTaxRateAuditService.RecordDelete,
            auditToDto: a => a.ToDto());

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
}