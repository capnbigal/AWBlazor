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

public static class CreditCardEndpoints
{
    public static IEndpointRouteBuilder MapCreditCardEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/aw/credit-cards")
            .WithTags("CreditCards")
            .RequireAuthorization("ApiOrCookie");

        group.MapGet("/", ListAsync).WithName("ListCreditCards").WithSummary("List Sales.CreditCard rows.");

        group.MapIntIdCrud<CreditCard, CreditCardDto, CreateCreditCardRequest, UpdateCreditCardRequest, CreditCardAuditLog, CreditCardAuditLogDto, CreditCardAuditService.Snapshot, int>(
            entityName: "CreditCard",
            routePrefix: "/api/aw/credit-cards",
            entitySet: db => db.CreditCards,
            auditSet: db => db.CreditCardAuditLogs,
            idSelector: e => e.Id,
            auditIdSelector: a => a.CreditCardId,
            auditChangedDateSelector: a => a.ChangedDate,
            auditPrimaryKeySelector: a => a.Id,
            getId: e => e.Id,
            toDto: e => e.ToDto(),
            toEntity: r => r.ToEntity(),
            applyUpdate: (r, e) => r.ApplyTo(e),
            captureSnapshot: CreditCardAuditService.CaptureSnapshot,
            recordCreate: CreditCardAuditService.RecordCreate,
            recordUpdate: CreditCardAuditService.RecordUpdate,
            recordDelete: CreditCardAuditService.RecordDelete,
            auditToDto: a => a.ToDto());

        return app;
    }

    private static async Task<Ok<PagedResult<CreditCardDto>>> ListAsync(
        ApplicationDbContext db, [FromQuery] int skip = 0, [FromQuery] int take = 50,
        [FromQuery] string? cardType = null, CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 1000);
        var query = db.CreditCards.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(cardType)) query = query.Where(x => x.CardType == cardType);
        var total = await query.CountAsync(ct);
        var rows = await query.OrderBy(x => x.Id).Skip(skip).Take(take).Select(x => x.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<CreditCardDto>(rows, total, skip, take));
    }
}