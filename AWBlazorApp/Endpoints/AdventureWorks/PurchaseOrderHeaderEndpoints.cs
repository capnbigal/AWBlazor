using System.Security.Claims;
using AWBlazorApp.Data;
using AWBlazorApp.Models;
using AWBlazorApp.Models.AdventureWorks;
using AWBlazorApp.Services.AdventureWorksAudit;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Endpoints.AdventureWorks;

public static class PurchaseOrderHeaderEndpoints
{
    public static IEndpointRouteBuilder MapPurchaseOrderHeaderEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/aw/purchase-order-headers")
            .WithTags("PurchaseOrderHeaders")
            .RequireAuthorization("ApiOrCookie");

        group.MapGet("/", ListAsync).WithName("ListPurchaseOrderHeaders").WithSummary("List Purchasing.PurchaseOrderHeader rows.");
        group.MapGet("/{id:int}", GetAsync).WithName("GetPurchaseOrderHeader");
        group.MapPost("/", CreateAsync).WithName("CreatePurchaseOrderHeader")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));
        group.MapPatch("/{id:int}", UpdateAsync).WithName("UpdatePurchaseOrderHeader")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));
        group.MapDelete("/{id:int}", DeleteAsync).WithName("DeletePurchaseOrderHeader")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));
        group.MapGet("/{id:int}/history", HistoryAsync).WithName("ListPurchaseOrderHeaderHistory");
        return app;
    }

    private static async Task<Ok<PagedResult<PurchaseOrderHeaderDto>>> ListAsync(
        ApplicationDbContext db, [FromQuery] int skip = 0, [FromQuery] int take = 50,
        [FromQuery] int? vendorId = null, [FromQuery] byte? status = null, CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 1000);
        var query = db.PurchaseOrderHeaders.AsNoTracking();
        if (vendorId.HasValue) query = query.Where(x => x.VendorId == vendorId.Value);
        if (status.HasValue) query = query.Where(x => x.Status == status.Value);
        var total = await query.CountAsync(ct);
        var rows = await query.OrderByDescending(x => x.OrderDate).ThenBy(x => x.Id)
            .Skip(skip).Take(take).Select(x => x.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<PurchaseOrderHeaderDto>(rows, total, skip, take));
    }

    private static async Task<Results<Ok<PurchaseOrderHeaderDto>, NotFound>> GetAsync(int id, ApplicationDbContext db, CancellationToken ct)
    {
        var row = await db.PurchaseOrderHeaders.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        return row is null ? TypedResults.NotFound() : TypedResults.Ok(row.ToDto());
    }

    private static async Task<Results<Created<IdResponse>, ValidationProblem>> CreateAsync(
        CreatePurchaseOrderHeaderRequest request, IValidator<CreatePurchaseOrderHeaderRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());

        var entity = request.ToEntity();
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        db.PurchaseOrderHeaders.Add(entity);
        await db.SaveChangesAsync(ct);
        db.PurchaseOrderHeaderAuditLogs.Add(PurchaseOrderHeaderAuditService.RecordCreate(entity, user.Identity?.Name));
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return TypedResults.Created($"/api/aw/purchase-order-headers/{entity.Id}", new IdResponse(entity.Id));
    }

    private static async Task<Results<Ok<IdResponse>, NotFound, ValidationProblem>> UpdateAsync(
        int id, UpdatePurchaseOrderHeaderRequest request, IValidator<UpdatePurchaseOrderHeaderRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());

        var entity = await db.PurchaseOrderHeaders.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null) return TypedResults.NotFound();

        var before = PurchaseOrderHeaderAuditService.CaptureSnapshot(entity);
        request.ApplyTo(entity);
        db.PurchaseOrderHeaderAuditLogs.Add(PurchaseOrderHeaderAuditService.RecordUpdate(before, entity, user.Identity?.Name));
        await db.SaveChangesAsync(ct);
        return TypedResults.Ok(new IdResponse(entity.Id));
    }

    private static async Task<Results<NoContent, NotFound>> DeleteAsync(
        int id, ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var entity = await db.PurchaseOrderHeaders.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null) return TypedResults.NotFound();

        db.PurchaseOrderHeaderAuditLogs.Add(PurchaseOrderHeaderAuditService.RecordDelete(entity, user.Identity?.Name));
        db.PurchaseOrderHeaders.Remove(entity);
        await db.SaveChangesAsync(ct);
        return TypedResults.NoContent();
    }

    private static async Task<Ok<List<PurchaseOrderHeaderAuditLogDto>>> HistoryAsync(int id, ApplicationDbContext db, CancellationToken ct)
    {
        var rows = await db.PurchaseOrderHeaderAuditLogs.AsNoTracking()
            .Where(a => a.PurchaseOrderId == id)
            .OrderByDescending(a => a.ChangedDate).ThenByDescending(a => a.Id)
            .Select(a => a.ToDto())
            .ToListAsync(ct);
        return TypedResults.Ok(rows);
    }
}
