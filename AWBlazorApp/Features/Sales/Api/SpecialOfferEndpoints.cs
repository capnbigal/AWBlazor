using AWBlazorApp.Features.Identity.Domain; using AWBlazorApp.Features.Admin.Permissions.Domain;
using AWBlazorApp.Infrastructure.Persistence;
using AWBlazorApp.Shared.Api;
using AWBlazorApp.Shared.Dtos;
using AWBlazorApp.Features.Sales.Audit;
using AWBlazorApp.Features.Sales.Domain;
using AWBlazorApp.Features.Sales.Dtos;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.Sales.Api;

public static class SpecialOfferEndpoints
{
    public static IEndpointRouteBuilder MapSpecialOfferEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/aw/special-offers")
            .WithTags("SpecialOffers")
            .RequireAuthorization("ApiOrCookie");

        group.MapGet("/", ListAsync).WithName("ListSpecialOffers").WithSummary("List Sales.SpecialOffer rows.");

        group.MapIntIdCrud<SpecialOffer, SpecialOfferDto, CreateSpecialOfferRequest, UpdateSpecialOfferRequest, SpecialOfferAuditLog, SpecialOfferAuditLogDto, SpecialOfferAuditService.Snapshot, int>(
            entityName: "SpecialOffer",
            routePrefix: "/api/aw/special-offers",
            entitySet: db => db.SpecialOffers,
            auditSet: db => db.SpecialOfferAuditLogs,
            idSelector: e => e.Id,
            auditIdSelector: a => a.SpecialOfferId,
            auditChangedDateSelector: a => a.ChangedDate,
            auditPrimaryKeySelector: a => a.Id,
            getId: e => e.Id,
            toDto: e => e.ToDto(),
            toEntity: r => r.ToEntity(),
            applyUpdate: (r, e) => r.ApplyTo(e),
            captureSnapshot: SpecialOfferAuditService.CaptureSnapshot,
            recordCreate: SpecialOfferAuditService.RecordCreate,
            recordUpdate: SpecialOfferAuditService.RecordUpdate,
            recordDelete: SpecialOfferAuditService.RecordDelete,
            auditToDto: a => a.ToDto());

        return app;
    }

    private static async Task<Ok<PagedResult<SpecialOfferDto>>> ListAsync(
        ApplicationDbContext db, [FromQuery] int skip = 0, [FromQuery] int take = 50,
        [FromQuery] string? description = null, [FromQuery] string? category = null, CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 1000);
        var query = db.SpecialOffers.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(description)) query = query.Where(x => x.Description.Contains(description));
        if (!string.IsNullOrWhiteSpace(category)) query = query.Where(x => x.Category == category);
        var total = await query.CountAsync(ct);
        var rows = await query.OrderBy(x => x.Id).Skip(skip).Take(take).Select(x => x.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<SpecialOfferDto>(rows, total, skip, take));
    }
}