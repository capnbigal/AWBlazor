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

public static class TransactionHistoryEndpoints
{
    public static IEndpointRouteBuilder MapTransactionHistoryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/aw/transaction-histories")
            .WithTags("TransactionHistories")
            .RequireAuthorization("ApiOrCookie");

        group.MapGet("/", ListAsync).WithName("ListTransactionHistories").WithSummary("List Production.TransactionHistory rows.");

        group.MapIntIdCrud<TransactionHistory, TransactionHistoryDto, CreateTransactionHistoryRequest, UpdateTransactionHistoryRequest, TransactionHistoryAuditLog, TransactionHistoryAuditLogDto, TransactionHistoryAuditService.Snapshot, int>(
            entityName: "TransactionHistory",
            routePrefix: "/api/aw/transaction-histories",
            entitySet: db => db.TransactionHistories,
            auditSet: db => db.TransactionHistoryAuditLogs,
            idSelector: e => e.Id,
            auditIdSelector: a => a.TransactionId,
            auditChangedDateSelector: a => a.ChangedDate,
            auditPrimaryKeySelector: a => a.Id,
            getId: e => e.Id,
            toDto: e => e.ToDto(),
            toEntity: r => r.ToEntity(),
            applyUpdate: (r, e) => r.ApplyTo(e),
            captureSnapshot: TransactionHistoryAuditService.CaptureSnapshot,
            recordCreate: TransactionHistoryAuditService.RecordCreate,
            recordUpdate: TransactionHistoryAuditService.RecordUpdate,
            recordDelete: TransactionHistoryAuditService.RecordDelete,
            auditToDto: a => a.ToDto());

        return app;
    }

    private static async Task<Ok<PagedResult<TransactionHistoryDto>>> ListAsync(
        ApplicationDbContext db, [FromQuery] int skip = 0, [FromQuery] int take = 50,
        [FromQuery] int? productId = null, CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 1000);
        var query = db.TransactionHistories.AsNoTracking();
        if (productId.HasValue) query = query.Where(x => x.ProductId == productId.Value);
        var total = await query.CountAsync(ct);
        var rows = await query.OrderByDescending(x => x.Id).Skip(skip).Take(take).Select(x => x.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<TransactionHistoryDto>(rows, total, skip, take));
    }
}