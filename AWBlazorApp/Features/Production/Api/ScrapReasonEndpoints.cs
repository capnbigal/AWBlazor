using AWBlazorApp.Features.Identity.Domain; using AWBlazorApp.Features.Admin.Permissions.Domain;
using AWBlazorApp.Infrastructure.Persistence;
using AWBlazorApp.Shared.Api;
using AWBlazorApp.Shared.Dtos;
using AWBlazorApp.Features.Production.Audit;
using AWBlazorApp.Features.Production.Domain;
using AWBlazorApp.Features.Production.Dtos;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.Production.Api;

public static class ScrapReasonEndpoints
{
    public static IEndpointRouteBuilder MapScrapReasonEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/aw/scrap-reasons")
            .WithTags("ScrapReasons")
            .RequireAuthorization("ApiOrCookie");

        group.MapGet("/", ListAsync).WithName("ListScrapReasons").WithSummary("List Production.ScrapReason rows.");

        group.MapIntIdCrud<ScrapReason, ScrapReasonDto, CreateScrapReasonRequest, UpdateScrapReasonRequest, ScrapReasonAuditLog, ScrapReasonAuditLogDto, ScrapReasonAuditService.Snapshot, short>(
            entityName: "ScrapReason",
            routePrefix: "/api/aw/scrap-reasons",
            entitySet: db => db.ScrapReasons,
            auditSet: db => db.ScrapReasonAuditLogs,
            idSelector: e => e.Id,
            auditIdSelector: a => a.ScrapReasonId,
            auditChangedDateSelector: a => a.ChangedDate,
            auditPrimaryKeySelector: a => a.Id,
            getId: e => e.Id,
            toDto: e => e.ToDto(),
            toEntity: r => r.ToEntity(),
            applyUpdate: (r, e) => r.ApplyTo(e),
            captureSnapshot: ScrapReasonAuditService.CaptureSnapshot,
            recordCreate: ScrapReasonAuditService.RecordCreate,
            recordUpdate: ScrapReasonAuditService.RecordUpdate,
            recordDelete: ScrapReasonAuditService.RecordDelete,
            auditToDto: a => a.ToDto());

        return app;
    }

    private static async Task<Ok<PagedResult<ScrapReasonDto>>> ListAsync(
        ApplicationDbContext db, [FromQuery] int skip = 0, [FromQuery] int take = 50,
        [FromQuery] string? name = null, CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 1000);
        var query = db.ScrapReasons.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(name)) query = query.Where(x => x.Name.Contains(name));
        var total = await query.CountAsync(ct);
        var rows = await query.OrderBy(x => x.Id).Skip(skip).Take(take).Select(x => x.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<ScrapReasonDto>(rows, total, skip, take));
    }
}