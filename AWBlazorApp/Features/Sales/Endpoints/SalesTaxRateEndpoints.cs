using AWBlazorApp.Data;
using AWBlazorApp.Infrastructure.Persistence;
using AWBlazorApp.Shared.Endpoints;
using AWBlazorApp.Shared.Models;
using AWBlazorApp.Features.Sales.Audit;
using AWBlazorApp.Features.Sales.Domain;
using AWBlazorApp.Features.Sales.Models;
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