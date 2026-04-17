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

public static class ShipmentEndpoints
{
    public static IEndpointRouteBuilder MapShipmentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/shipments")
            .WithTags("Shipments")
            .RequireAuthorization("ApiOrCookie");

        group.MapGet("/", ListAsync).WithName("ListShipments");
        group.MapGet("/{id:int}", GetAsync).WithName("GetShipment");
        group.MapPost("/", CreateAsync).WithName("CreateShipment")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));
        group.MapPatch("/{id:int}", UpdateAsync).WithName("UpdateShipment")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));
        group.MapDelete("/{id:int}", DeleteAsync).WithName("DeleteShipment")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));
        group.MapPost("/{id:int}/post", PostShipmentAsync).WithName("PostShipment")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));
        group.MapGet("/{id:int}/history", HistoryAsync).WithName("ListShipmentHistory");

        group.MapGet("/{id:int}/lines", ListLinesAsync).WithName("ListShipmentLines");
        group.MapPost("/{id:int}/lines", CreateLineAsync).WithName("CreateShipmentLine")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));
        group.MapPatch("/lines/{lineId:int}", UpdateLineAsync).WithName("UpdateShipmentLine")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));
        group.MapDelete("/lines/{lineId:int}", DeleteLineAsync).WithName("DeleteShipmentLine")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));

        return app;
    }

    private static async Task<Ok<PagedResult<ShipmentDto>>> ListAsync(
        ApplicationDbContext db,
        [FromQuery] int skip = 0, [FromQuery] int take = 50,
        [FromQuery] ShipmentStatus? status = null,
        [FromQuery] int? salesOrderId = null,
        [FromQuery] int? shippedFromLocationId = null,
        CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 500);
        var q = db.Shipments.AsNoTracking();
        if (status.HasValue) q = q.Where(x => x.Status == status.Value);
        if (salesOrderId.HasValue) q = q.Where(x => x.SalesOrderId == salesOrderId.Value);
        if (shippedFromLocationId.HasValue) q = q.Where(x => x.ShippedFromLocationId == shippedFromLocationId.Value);
        var total = await q.CountAsync(ct);
        var rows = await q.OrderByDescending(x => x.ModifiedDate).ThenByDescending(x => x.Id)
            .Skip(skip).Take(take).Select(x => x.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<ShipmentDto>(rows, total, skip, take));
    }

    private static async Task<Results<Ok<ShipmentDto>, NotFound>> GetAsync(int id, ApplicationDbContext db, CancellationToken ct)
    {
        var row = await db.Shipments.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        return row is null ? TypedResults.NotFound() : TypedResults.Ok(row.ToDto());
    }

    private static async Task<Results<Created<IdResponse>, ValidationProblem>> CreateAsync(
        CreateShipmentRequest request,
        IValidator<CreateShipmentRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());
        var entity = request.ToEntity();
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        db.Shipments.Add(entity);
        await db.SaveChangesAsync(ct);
        db.ShipmentAuditLogs.Add(ShipmentAuditService.RecordCreate(entity, user.Identity?.Name));
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return TypedResults.Created($"/api/shipments/{entity.Id}", new IdResponse(entity.Id));
    }

    private static async Task<Results<Ok<IdResponse>, NotFound, ValidationProblem>> UpdateAsync(
        int id, UpdateShipmentRequest request,
        IValidator<UpdateShipmentRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());
        var entity = await db.Shipments.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null) return TypedResults.NotFound();

        var before = ShipmentAuditService.CaptureSnapshot(entity);
        request.ApplyTo(entity);
        db.ShipmentAuditLogs.Add(ShipmentAuditService.RecordUpdate(before, entity, user.Identity?.Name));
        await db.SaveChangesAsync(ct);
        return TypedResults.Ok(new IdResponse(entity.Id));
    }

    private static async Task<Results<NoContent, NotFound, ValidationProblem>> DeleteAsync(
        int id, ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var entity = await db.Shipments.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null) return TypedResults.NotFound();
        if (entity.Status is ShipmentStatus.Shipped or ShipmentStatus.Delivered)
            return TypedResults.ValidationProblem(new Dictionary<string, string[]> { ["Status"] = ["Cannot delete a shipment that has left the warehouse."] });

        await using var tx = await db.Database.BeginTransactionAsync(ct);
        db.Shipments.Remove(entity);
        db.ShipmentAuditLogs.Add(ShipmentAuditService.RecordDelete(entity, user.Identity?.Name));
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return TypedResults.NoContent();
    }

    private static async Task<Results<Ok<PostingResult>, NotFound, BadRequest<string>>> PostShipmentAsync(
        int id, ILogisticsPostingService service, ClaimsPrincipal user, CancellationToken ct)
    {
        try { return TypedResults.Ok(await service.PostShipmentAsync(id, user.Identity?.Name, ct)); }
        catch (InvalidOperationException ex) { return TypedResults.BadRequest(ex.Message); }
    }

    private static async Task<Ok<PagedResult<ShipmentAuditLogDto>>> HistoryAsync(
        int id, ApplicationDbContext db,
        [FromQuery] int skip = 0, [FromQuery] int take = 50, CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 500);
        var q = db.ShipmentAuditLogs.AsNoTracking().Where(a => a.ShipmentId == id);
        var total = await q.CountAsync(ct);
        var rows = await q.OrderByDescending(a => a.ChangedDate).ThenByDescending(a => a.Id)
            .Skip(skip).Take(take).Select(a => a.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<ShipmentAuditLogDto>(rows, total, skip, take));
    }

    private static async Task<Ok<PagedResult<ShipmentLineDto>>> ListLinesAsync(
        int id, ApplicationDbContext db,
        [FromQuery] int skip = 0, [FromQuery] int take = 500, CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 1000);
        var q = db.ShipmentLines.AsNoTracking().Where(l => l.ShipmentId == id);
        var total = await q.CountAsync(ct);
        var rows = await q.OrderBy(l => l.Id).Skip(skip).Take(take).Select(l => l.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<ShipmentLineDto>(rows, total, skip, take));
    }

    private static async Task<Results<Created<IdResponse>, NotFound, ValidationProblem>> CreateLineAsync(
        int id, CreateShipmentLineRequest request,
        IValidator<CreateShipmentLineRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());
        var header = await db.Shipments.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (header is null) return TypedResults.NotFound();
        if (header.Status is ShipmentStatus.Shipped or ShipmentStatus.Delivered)
            return TypedResults.ValidationProblem(new Dictionary<string, string[]> { ["Status"] = ["Shipment has already left; no more lines."] });

        request = request with { ShipmentId = id };
        var entity = request.ToEntity();
        db.ShipmentLines.Add(entity);
        await db.SaveChangesAsync(ct);
        db.ShipmentLineAuditLogs.Add(ShipmentLineAuditService.RecordCreate(entity, user.Identity?.Name));
        await db.SaveChangesAsync(ct);
        return TypedResults.Created($"/api/shipments/{id}/lines/{entity.Id}", new IdResponse(entity.Id));
    }

    private static async Task<Results<Ok<IdResponse>, NotFound, ValidationProblem>> UpdateLineAsync(
        int lineId, UpdateShipmentLineRequest request,
        IValidator<UpdateShipmentLineRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());
        var entity = await db.ShipmentLines.FirstOrDefaultAsync(x => x.Id == lineId, ct);
        if (entity is null) return TypedResults.NotFound();

        var before = ShipmentLineAuditService.CaptureSnapshot(entity);
        request.ApplyTo(entity);
        db.ShipmentLineAuditLogs.Add(ShipmentLineAuditService.RecordUpdate(before, entity, user.Identity?.Name));
        await db.SaveChangesAsync(ct);
        return TypedResults.Ok(new IdResponse(entity.Id));
    }

    private static async Task<Results<NoContent, NotFound>> DeleteLineAsync(
        int lineId, ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var entity = await db.ShipmentLines.FirstOrDefaultAsync(x => x.Id == lineId, ct);
        if (entity is null) return TypedResults.NotFound();
        db.ShipmentLines.Remove(entity);
        db.ShipmentLineAuditLogs.Add(ShipmentLineAuditService.RecordDelete(entity, user.Identity?.Name));
        await db.SaveChangesAsync(ct);
        return TypedResults.NoContent();
    }
}
