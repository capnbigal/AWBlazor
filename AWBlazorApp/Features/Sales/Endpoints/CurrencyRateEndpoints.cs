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