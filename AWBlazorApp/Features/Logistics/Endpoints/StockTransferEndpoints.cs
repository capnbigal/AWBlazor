using System.Security.Claims;
using AWBlazorApp.Data;
using AWBlazorApp.Infrastructure.Persistence;
using AWBlazorApp.Shared.Models;
using AWBlazorApp.Features.Logistics.Audit;
using AWBlazorApp.Features.Logistics.Domain;
using AWBlazorApp.Features.Logistics.Models;
using AWBlazorApp.Features.Logistics.Services;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.Logistics.Endpoints;

public static class StockTransferEndpoints
{
    public static IEndpointRouteBuilder MapStockTransferEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/stock-transfers")
            .WithTags("StockTransfers")
            .RequireAuthorization("ApiOrCookie");

        group.MapGet("/", ListAsync).WithName("ListStockTransfers");
        group.MapGet("/{id:int}", GetAsync).WithName("GetStockTransfer");
        group.MapPost("/", CreateAsync).WithName("CreateStockTransfer")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));
        group.MapPatch("/{id:int}", UpdateAsync).WithName("UpdateStockTransfer")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));
        group.MapDelete("/{id:int}", DeleteAsync).WithName("DeleteStockTransfer")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));
        group.MapPost("/{id:int}/post", PostTransferAsync).WithName("PostStockTransfer")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));
        group.MapGet("/{id:int}/history", HistoryAsync).WithName("ListStockTransferHistory");

        group.MapGet("/{id:int}/lines", ListLinesAsync).WithName("ListStockTransferLines");
        group.MapPost("/{id:int}/lines", CreateLineAsync).WithName("CreateStockTransferLine")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));
        group.MapPatch("/lines/{lineId:int}", UpdateLineAsync).WithName("UpdateStockTransferLine")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));
        group.MapDelete("/lines/{lineId:int}", DeleteLineAsync).WithName("DeleteStockTransferLine")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));

        return app;
    }

    private static async Task<Ok<PagedResult<StockTransferDto>>> ListAsync(
        ApplicationDbContext db,
        [FromQuery] int skip = 0, [FromQuery] int take = 50,
        [FromQuery] StockTransferStatus? status = null,
        [FromQuery] int? fromLocationId = null,
        [FromQuery] int? toLocationId = null,
        CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 500);
        var q = db.StockTransfers.AsNoTracking();
        if (status.HasValue) q = q.Where(x => x.Status == status.Value);
        if (fromLocationId.HasValue) q = q.Where(x => x.FromLocationId == fromLocationId.Value);
        if (toLocationId.HasValue) q = q.Where(x => x.ToLocationId == toLocationId.Value);
        var total = await q.CountAsync(ct);
        var rows = await q.OrderByDescending(x => x.InitiatedAt).ThenByDescending(x => x.Id)
            .Skip(skip).Take(take).Select(x => x.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<StockTransferDto>(rows, total, skip, take));
    }

    private static async Task<Results<Ok<StockTransferDto>, NotFound>> GetAsync(int id, ApplicationDbContext db, CancellationToken ct)
    {
        var row = await db.StockTransfers.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        return row is null ? TypedResults.NotFound() : TypedResults.Ok(row.ToDto());
    }

    private static async Task<Results<Created<IdResponse>, ValidationProblem>> CreateAsync(
        CreateStockTransferRequest request,
        IValidator<CreateStockTransferRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());
        var entity = request.ToEntity();
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        db.StockTransfers.Add(entity);
        await db.SaveChangesAsync(ct);
        db.StockTransferAuditLogs.Add(StockTransferAuditService.RecordCreate(entity, user.Identity?.Name));
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return TypedResults.Created($"/api/stock-transfers/{entity.Id}", new IdResponse(entity.Id));
    }

    private static async Task<Results<Ok<IdResponse>, NotFound, ValidationProblem>> UpdateAsync(
        int id, UpdateStockTransferRequest request,
        IValidator<UpdateStockTransferRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());
        var entity = await db.StockTransfers.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null) return TypedResults.NotFound();
        if (entity.Status is StockTransferStatus.Completed or StockTransferStatus.Cancelled)
            return TypedResults.ValidationProblem(new Dictionary<string, string[]> { ["Status"] = [$"Transfer is {entity.Status}; no further edits."] });

        var before = StockTransferAuditService.CaptureSnapshot(entity);
        request.ApplyTo(entity);
        db.StockTransferAuditLogs.Add(StockTransferAuditService.RecordUpdate(before, entity, user.Identity?.Name));
        await db.SaveChangesAsync(ct);
        return TypedResults.Ok(new IdResponse(entity.Id));
    }

    private static async Task<Results<NoContent, NotFound, ValidationProblem>> DeleteAsync(
        int id, ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var entity = await db.StockTransfers.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null) return TypedResults.NotFound();
        if (entity.Status == StockTransferStatus.Completed)
            return TypedResults.ValidationProblem(new Dictionary<string, string[]> { ["Status"] = ["Cannot delete a completed transfer."] });

        await using var tx = await db.Database.BeginTransactionAsync(ct);
        db.StockTransfers.Remove(entity);
        db.StockTransferAuditLogs.Add(StockTransferAuditService.RecordDelete(entity, user.Identity?.Name));
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return TypedResults.NoContent();
    }

    private static async Task<Results<Ok<PostingResult>, NotFound, BadRequest<string>>> PostTransferAsync(
        int id, ILogisticsPostingService service, ClaimsPrincipal user, CancellationToken ct)
    {
        try { return TypedResults.Ok(await service.PostTransferAsync(id, user.Identity?.Name, ct)); }
        catch (InvalidOperationException ex) { return TypedResults.BadRequest(ex.Message); }
    }

    private static async Task<Ok<PagedResult<StockTransferAuditLogDto>>> HistoryAsync(
        int id, ApplicationDbContext db,
        [FromQuery] int skip = 0, [FromQuery] int take = 50, CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 500);
        var q = db.StockTransferAuditLogs.AsNoTracking().Where(a => a.StockTransferId == id);
        var total = await q.CountAsync(ct);
        var rows = await q.OrderByDescending(a => a.ChangedDate).ThenByDescending(a => a.Id)
            .Skip(skip).Take(take).Select(a => a.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<StockTransferAuditLogDto>(rows, total, skip, take));
    }

    private static async Task<Ok<PagedResult<StockTransferLineDto>>> ListLinesAsync(
        int id, ApplicationDbContext db,
        [FromQuery] int skip = 0, [FromQuery] int take = 500, CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 1000);
        var q = db.StockTransferLines.AsNoTracking().Where(l => l.StockTransferId == id);
        var total = await q.CountAsync(ct);
        var rows = await q.OrderBy(l => l.Id).Skip(skip).Take(take).Select(l => l.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<StockTransferLineDto>(rows, total, skip, take));
    }

    private static async Task<Results<Created<IdResponse>, NotFound, ValidationProblem>> CreateLineAsync(
        int id, CreateStockTransferLineRequest request,
        IValidator<CreateStockTransferLineRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());
        var header = await db.StockTransfers.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (header is null) return TypedResults.NotFound();
        if (header.Status is StockTransferStatus.Completed or StockTransferStatus.Cancelled)
            return TypedResults.ValidationProblem(new Dictionary<string, string[]> { ["Status"] = ["Transfer is closed; no more lines."] });

        request = request with { StockTransferId = id };
        var entity = request.ToEntity();
        db.StockTransferLines.Add(entity);
        await db.SaveChangesAsync(ct);
        db.StockTransferLineAuditLogs.Add(StockTransferLineAuditService.RecordCreate(entity, user.Identity?.Name));
        await db.SaveChangesAsync(ct);
        return TypedResults.Created($"/api/stock-transfers/{id}/lines/{entity.Id}", new IdResponse(entity.Id));
    }

    private static async Task<Results<Ok<IdResponse>, NotFound, ValidationProblem>> UpdateLineAsync(
        int lineId, UpdateStockTransferLineRequest request,
        IValidator<UpdateStockTransferLineRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());
        var entity = await db.StockTransferLines.FirstOrDefaultAsync(x => x.Id == lineId, ct);
        if (entity is null) return TypedResults.NotFound();

        var before = StockTransferLineAuditService.CaptureSnapshot(entity);
        request.ApplyTo(entity);
        db.StockTransferLineAuditLogs.Add(StockTransferLineAuditService.RecordUpdate(before, entity, user.Identity?.Name));
        await db.SaveChangesAsync(ct);
        return TypedResults.Ok(new IdResponse(entity.Id));
    }

    private static async Task<Results<NoContent, NotFound>> DeleteLineAsync(
        int lineId, ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var entity = await db.StockTransferLines.FirstOrDefaultAsync(x => x.Id == lineId, ct);
        if (entity is null) return TypedResults.NotFound();
        db.StockTransferLines.Remove(entity);
        db.StockTransferLineAuditLogs.Add(StockTransferLineAuditService.RecordDelete(entity, user.Identity?.Name));
        await db.SaveChangesAsync(ct);
        return TypedResults.NoContent();
    }
}
