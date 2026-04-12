using System.Security.Claims;
using ElementaryApp.Data;
using ElementaryApp.Models;
using ElementaryApp.Models.AdventureWorks;
using ElementaryApp.Services.AdventureWorksAudit;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ElementaryApp.Endpoints.AdventureWorks;

public static class TransactionHistoryArchiveEndpoints
{
    public static IEndpointRouteBuilder MapTransactionHistoryArchiveEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/aw/transaction-history-archives")
            .WithTags("TransactionHistoryArchives")
            .RequireAuthorization("ApiOrCookie");

        group.MapGet("/", ListAsync).WithName("ListTransactionHistoryArchives").WithSummary("List Production.TransactionHistoryArchive rows.");
        group.MapGet("/{id:int}", GetAsync).WithName("GetTransactionHistoryArchive");
        group.MapPost("/", CreateAsync).WithName("CreateTransactionHistoryArchive")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));
        group.MapPatch("/{id:int}", UpdateAsync).WithName("UpdateTransactionHistoryArchive")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));
        group.MapDelete("/{id:int}", DeleteAsync).WithName("DeleteTransactionHistoryArchive")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));
        group.MapGet("/{id:int}/history", HistoryAsync).WithName("ListTransactionHistoryArchiveHistory");
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

    private static async Task<Results<Ok<TransactionHistoryArchiveDto>, NotFound>> GetAsync(int id, ApplicationDbContext db, CancellationToken ct)
    {
        var row = await db.TransactionHistoryArchives.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        return row is null ? TypedResults.NotFound() : TypedResults.Ok(row.ToDto());
    }

    private static async Task<Results<Created<IdResponse>, Conflict<string>, ValidationProblem>> CreateAsync(
        CreateTransactionHistoryArchiveRequest request, IValidator<CreateTransactionHistoryArchiveRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());

        if (await db.TransactionHistoryArchives.AnyAsync(x => x.Id == request.Id, ct))
            return TypedResults.Conflict($"TransactionHistoryArchive with Id {request.Id} already exists.");

        var entity = request.ToEntity();
        db.TransactionHistoryArchives.Add(entity);
        await db.SaveChangesAsync(ct);
        db.TransactionHistoryArchiveAuditLogs.Add(TransactionHistoryArchiveAuditService.RecordCreate(entity, user.Identity?.Name));
        await db.SaveChangesAsync(ct);
        return TypedResults.Created($"/api/aw/transaction-history-archives/{entity.Id}", new IdResponse(entity.Id));
    }

    private static async Task<Results<Ok<IdResponse>, NotFound, ValidationProblem>> UpdateAsync(
        int id, UpdateTransactionHistoryArchiveRequest request, IValidator<UpdateTransactionHistoryArchiveRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());

        var entity = await db.TransactionHistoryArchives.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null) return TypedResults.NotFound();

        var before = TransactionHistoryArchiveAuditService.CaptureSnapshot(entity);
        request.ApplyTo(entity);
        db.TransactionHistoryArchiveAuditLogs.Add(TransactionHistoryArchiveAuditService.RecordUpdate(before, entity, user.Identity?.Name));
        await db.SaveChangesAsync(ct);
        return TypedResults.Ok(new IdResponse(entity.Id));
    }

    private static async Task<Results<NoContent, NotFound>> DeleteAsync(
        int id, ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var entity = await db.TransactionHistoryArchives.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null) return TypedResults.NotFound();

        db.TransactionHistoryArchiveAuditLogs.Add(TransactionHistoryArchiveAuditService.RecordDelete(entity, user.Identity?.Name));
        db.TransactionHistoryArchives.Remove(entity);
        await db.SaveChangesAsync(ct);
        return TypedResults.NoContent();
    }

    private static async Task<Ok<List<TransactionHistoryArchiveAuditLogDto>>> HistoryAsync(int id, ApplicationDbContext db, CancellationToken ct)
    {
        var rows = await db.TransactionHistoryArchiveAuditLogs.AsNoTracking()
            .Where(a => a.TransactionId == id)
            .OrderByDescending(a => a.ChangedDate).ThenByDescending(a => a.Id)
            .Select(a => a.ToDto())
            .ToListAsync(ct);
        return TypedResults.Ok(rows);
    }
}
