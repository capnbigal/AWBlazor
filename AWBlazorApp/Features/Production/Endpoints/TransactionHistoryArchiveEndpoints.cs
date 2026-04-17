using AWBlazorApp.Data;
using AWBlazorApp.Infrastructure.Persistence;
using AWBlazorApp.Shared.Endpoints;
using AWBlazorApp.Shared.Models;
using AWBlazorApp.Features.Production.Audit;
using AWBlazorApp.Features.Production.Domain;
using AWBlazorApp.Features.Production.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.Production.Endpoints;

public static class TransactionHistoryArchiveEndpoints
{
    public static IEndpointRouteBuilder MapTransactionHistoryArchiveEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/aw/transaction-history-archives")
            .WithTags("TransactionHistoryArchives")
            .RequireAuthorization("ApiOrCookie");

        group.MapGet("/", ListAsync).WithName("ListTransactionHistoryArchives").WithSummary("List Production.TransactionHistoryArchive rows.");

        group.MapIntIdCrud<TransactionHistoryArchive, TransactionHistoryArchiveDto, CreateTransactionHistoryArchiveRequest, UpdateTransactionHistoryArchiveRequest, TransactionHistoryArchiveAuditLog, TransactionHistoryArchiveAuditLogDto, TransactionHistoryArchiveAuditService.Snapshot, int>(
            entityName: "TransactionHistoryArchive",
            routePrefix: "/api/aw/transaction-history-archives",
            entitySet: db => db.TransactionHistoryArchives,
            auditSet: db => db.TransactionHistoryArchiveAuditLogs,
            idSelector: e => e.Id,
            auditIdSelector: a => a.TransactionId,
            auditChangedDateSelector: a => a.ChangedDate,
            auditPrimaryKeySelector: a => a.Id,
            getId: e => e.Id,
            toDto: e => e.ToDto(),
            toEntity: r => r.ToEntity(),
            applyUpdate: (r, e) => r.ApplyTo(e),
            captureSnapshot: TransactionHistoryArchiveAuditService.CaptureSnapshot,
            recordCreate: TransactionHistoryArchiveAuditService.RecordCreate,
            recordUpdate: TransactionHistoryArchiveAuditService.RecordUpdate,
            recordDelete: TransactionHistoryArchiveAuditService.RecordDelete,
            auditToDto: a => a.ToDto());

        return app;
    }

    private static async Task<Ok<PagedResult<TransactionHistoryArchiveDto>>> ListAsync(
        ApplicationDbContext db, [FromQuery] int skip = 0, [FromQuery] int take = 50,
        [FromQuery] int? productId = null, CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 1000);
        var query = db.TransactionHistoryArchives.AsNoTracking();
        if (productId.HasValue) query = query.Where(x => x.ProductId == productId.Value);
        var total = await query.CountAsync(ct);
        var rows = await query.OrderByDescending(x => x.Id).Skip(skip).Take(take).Select(x => x.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<TransactionHistoryArchiveDto>(rows, total, skip, take));
    }
}