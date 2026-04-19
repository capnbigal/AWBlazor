using System.Security.Claims;
using AWBlazorApp.Features.Identity.Domain; using AWBlazorApp.Features.Admin.Permissions.Domain;
using AWBlazorApp.Infrastructure.Persistence;
using AWBlazorApp.Shared.Dtos;
using AWBlazorApp.Features.Logistics.Audit;
using AWBlazorApp.Features.Logistics.Domain;
using AWBlazorApp.Features.Logistics.Dtos;
using AWBlazorApp.Features.Logistics.Services;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.Logistics.Api;

public static class GoodsReceiptEndpoints
{
    public static IEndpointRouteBuilder MapGoodsReceiptEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/goods-receipts")
            .WithTags("GoodsReceipts")
            .RequireAuthorization("ApiOrCookie");

        group.MapGet("/", ListAsync).WithName("ListGoodsReceipts");
        group.MapGet("/{id:int}", GetAsync).WithName("GetGoodsReceipt");
        group.MapPost("/", CreateAsync).WithName("CreateGoodsReceipt")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));
        group.MapPatch("/{id:int}", UpdateAsync).WithName("UpdateGoodsReceipt")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));
        group.MapDelete("/{id:int}", DeleteAsync).WithName("DeleteGoodsReceipt")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));
        group.MapPost("/{id:int}/post", PostReceiptAsync).WithName("PostGoodsReceipt")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));
        group.MapGet("/{id:int}/history", HistoryAsync).WithName("ListGoodsReceiptHistory");

        group.MapGet("/{id:int}/lines", ListLinesAsync).WithName("ListGoodsReceiptLines");
        group.MapPost("/{id:int}/lines", CreateLineAsync).WithName("CreateGoodsReceiptLine")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));
        group.MapPatch("/lines/{lineId:int}", UpdateLineAsync).WithName("UpdateGoodsReceiptLine")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));
        group.MapDelete("/lines/{lineId:int}", DeleteLineAsync).WithName("DeleteGoodsReceiptLine")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));

        return app;
    }

    private static async Task<Ok<PagedResult<GoodsReceiptDto>>> ListAsync(
        ApplicationDbContext db,
        [FromQuery] int skip = 0, [FromQuery] int take = 50,
        [FromQuery] GoodsReceiptStatus? status = null,
        [FromQuery] int? purchaseOrderId = null,
        [FromQuery] int? receivedLocationId = null,
        CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 500);
        var q = db.GoodsReceipts.AsNoTracking();
        if (status.HasValue) q = q.Where(x => x.Status == status.Value);
        if (purchaseOrderId.HasValue) q = q.Where(x => x.PurchaseOrderId == purchaseOrderId.Value);
        if (receivedLocationId.HasValue) q = q.Where(x => x.ReceivedLocationId == receivedLocationId.Value);
        var total = await q.CountAsync(ct);
        var rows = await q.OrderByDescending(x => x.ReceivedAt).ThenByDescending(x => x.Id)
            .Skip(skip).Take(take).Select(x => x.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<GoodsReceiptDto>(rows, total, skip, take));
    }

    private static async Task<Results<Ok<GoodsReceiptDto>, NotFound>> GetAsync(int id, ApplicationDbContext db, CancellationToken ct)
    {
        var row = await db.GoodsReceipts.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        return row is null ? TypedResults.NotFound() : TypedResults.Ok(row.ToDto());
    }

    private static async Task<Results<Created<IdResponse>, ValidationProblem>> CreateAsync(
        CreateGoodsReceiptRequest request,
        IValidator<CreateGoodsReceiptRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());

        var entity = request.ToEntity();
        await db.AddWithAuditAsync(entity, e => GoodsReceiptAuditService.RecordCreate(e, user.Identity?.Name), ct);
        return TypedResults.Created($"/api/goods-receipts/{entity.Id}", new IdResponse(entity.Id));
    }

    private static async Task<Results<Ok<IdResponse>, NotFound, ValidationProblem>> UpdateAsync(
        int id, UpdateGoodsReceiptRequest request,
        IValidator<UpdateGoodsReceiptRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());

        var entity = await db.GoodsReceipts.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null) return TypedResults.NotFound();
        if (entity.Status is GoodsReceiptStatus.Posted or GoodsReceiptStatus.Cancelled)
            return TypedResults.ValidationProblem(new Dictionary<string, string[]> { ["Status"] = [$"Receipt is {entity.Status}; no further edits allowed."] });

        var before = GoodsReceiptAuditService.CaptureSnapshot(entity);
        request.ApplyTo(entity);
        db.GoodsReceiptAuditLogs.Add(GoodsReceiptAuditService.RecordUpdate(before, entity, user.Identity?.Name));
        await db.SaveChangesAsync(ct);
        return TypedResults.Ok(new IdResponse(entity.Id));
    }

    private static async Task<Results<NoContent, NotFound, ValidationProblem>> DeleteAsync(
        int id, ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var entity = await db.GoodsReceipts.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null) return TypedResults.NotFound();
        if (entity.Status == GoodsReceiptStatus.Posted)
            return TypedResults.ValidationProblem(new Dictionary<string, string[]> { ["Status"] = ["Cannot delete a posted receipt."] });

        await db.DeleteWithAuditAsync(entity, GoodsReceiptAuditService.RecordDelete(entity, user.Identity?.Name), ct);
        return TypedResults.NoContent();
    }

    private static async Task<Results<Ok<PostingResult>, NotFound, BadRequest<string>>> PostReceiptAsync(
        int id, ILogisticsPostingService service, ClaimsPrincipal user, CancellationToken ct)
    {
        try { return TypedResults.Ok(await service.PostReceiptAsync(id, user.Identity?.Name, ct)); }
        catch (InvalidOperationException ex) { return TypedResults.BadRequest(ex.Message); }
    }

    private static async Task<Ok<PagedResult<GoodsReceiptAuditLogDto>>> HistoryAsync(
        int id, ApplicationDbContext db,
        [FromQuery] int skip = 0, [FromQuery] int take = 50, CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 500);
        var q = db.GoodsReceiptAuditLogs.AsNoTracking().Where(a => a.GoodsReceiptId == id);
        var total = await q.CountAsync(ct);
        var rows = await q.OrderByDescending(a => a.ChangedDate).ThenByDescending(a => a.Id)
            .Skip(skip).Take(take).Select(a => a.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<GoodsReceiptAuditLogDto>(rows, total, skip, take));
    }

    private static async Task<Ok<PagedResult<GoodsReceiptLineDto>>> ListLinesAsync(
        int id, ApplicationDbContext db,
        [FromQuery] int skip = 0, [FromQuery] int take = 500, CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 1000);
        var q = db.GoodsReceiptLines.AsNoTracking().Where(l => l.GoodsReceiptId == id);
        var total = await q.CountAsync(ct);
        var rows = await q.OrderBy(l => l.Id).Skip(skip).Take(take).Select(l => l.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<GoodsReceiptLineDto>(rows, total, skip, take));
    }

    private static async Task<Results<Created<IdResponse>, NotFound, ValidationProblem>> CreateLineAsync(
        int id, CreateGoodsReceiptLineRequest request,
        IValidator<CreateGoodsReceiptLineRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());
        var header = await db.GoodsReceipts.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (header is null) return TypedResults.NotFound();
        if (header.Status == GoodsReceiptStatus.Posted) return TypedResults.ValidationProblem(new Dictionary<string, string[]> { ["Status"] = ["Receipt is posted; no more lines."] });

        request = request with { } with { GoodsReceiptId = id };
        var entity = request.ToEntity();
        db.GoodsReceiptLines.Add(entity);
        await db.SaveChangesAsync(ct);
        db.GoodsReceiptLineAuditLogs.Add(GoodsReceiptLineAuditService.RecordCreate(entity, user.Identity?.Name));
        await db.SaveChangesAsync(ct);
        return TypedResults.Created($"/api/goods-receipts/{id}/lines/{entity.Id}", new IdResponse(entity.Id));
    }

    private static async Task<Results<Ok<IdResponse>, NotFound, ValidationProblem>> UpdateLineAsync(
        int lineId, UpdateGoodsReceiptLineRequest request,
        IValidator<UpdateGoodsReceiptLineRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());
        var entity = await db.GoodsReceiptLines.FirstOrDefaultAsync(x => x.Id == lineId, ct);
        if (entity is null) return TypedResults.NotFound();

        var before = GoodsReceiptLineAuditService.CaptureSnapshot(entity);
        request.ApplyTo(entity);
        db.GoodsReceiptLineAuditLogs.Add(GoodsReceiptLineAuditService.RecordUpdate(before, entity, user.Identity?.Name));
        await db.SaveChangesAsync(ct);
        return TypedResults.Ok(new IdResponse(entity.Id));
    }

    private static async Task<Results<NoContent, NotFound>> DeleteLineAsync(
        int lineId, ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var entity = await db.GoodsReceiptLines.FirstOrDefaultAsync(x => x.Id == lineId, ct);
        if (entity is null) return TypedResults.NotFound();
        db.GoodsReceiptLines.Remove(entity);
        db.GoodsReceiptLineAuditLogs.Add(GoodsReceiptLineAuditService.RecordDelete(entity, user.Identity?.Name));
        await db.SaveChangesAsync(ct);
        return TypedResults.NoContent();
    }
}
