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

public static class SalesTerritoryHistoryEndpoints
{
    public static IEndpointRouteBuilder MapSalesTerritoryHistoryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/aw/sales-territory-histories")
            .WithTags("SalesTerritoryHistories")
            .RequireAuthorization("ApiOrCookie");

        group.MapGet("/", ListAsync).WithName("ListSalesTerritoryHistories")
            .WithSummary("List Sales.SalesTerritoryHistory rows. Composite PK = (BusinessEntityID, StartDate, TerritoryID).");
        group.MapGet("/by-key", GetAsync).WithName("GetSalesTerritoryHistory");
        group.MapPost("/", CreateAsync).WithName("CreateSalesTerritoryHistory")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));
        group.MapPatch("/by-key", UpdateAsync).WithName("UpdateSalesTerritoryHistory")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));
        group.MapDelete("/by-key", DeleteAsync).WithName("DeleteSalesTerritoryHistory")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));
        group.MapGet("/history", HistoryAsync).WithName("ListSalesTerritoryHistoryHistory");
        return app;
    }

    private static async Task<Ok<PagedResult<SalesTerritoryHistoryDto>>> ListAsync(
        ApplicationDbContext db, [FromQuery] int skip = 0, [FromQuery] int take = 50,
        [FromQuery] int? businessEntityId = null, [FromQuery] int? territoryId = null,
        CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 1000);
        var query = db.SalesTerritoryHistories.AsNoTracking();
        if (businessEntityId.HasValue) query = query.Where(x => x.BusinessEntityId == businessEntityId.Value);
        if (territoryId.HasValue) query = query.Where(x => x.TerritoryId == territoryId.Value);
        var total = await query.CountAsync(ct);
        var rows = await query.OrderBy(x => x.BusinessEntityId).ThenByDescending(x => x.StartDate)
            .Skip(skip).Take(take).Select(x => x.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<SalesTerritoryHistoryDto>(rows, total, skip, take));
    }

    private static async Task<Results<Ok<SalesTerritoryHistoryDto>, NotFound>> GetAsync(
        [FromQuery] int businessEntityId, [FromQuery] DateTime startDate, [FromQuery] int territoryId,
        ApplicationDbContext db, CancellationToken ct)
    {
        var row = await db.SalesTerritoryHistories.AsNoTracking()
            .FirstOrDefaultAsync(x => x.BusinessEntityId == businessEntityId && x.StartDate == startDate && x.TerritoryId == territoryId, ct);
        return row is null ? TypedResults.NotFound() : TypedResults.Ok(row.ToDto());
    }

    private static async Task<Results<Created<CompositeKeyResponse>, Conflict<string>, ValidationProblem>> CreateAsync(
        CreateSalesTerritoryHistoryRequest request, IValidator<CreateSalesTerritoryHistoryRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());

        if (await db.SalesTerritoryHistories.AnyAsync(x =>
                x.BusinessEntityId == request.BusinessEntityId && x.StartDate == request.StartDate && x.TerritoryId == request.TerritoryId, ct))
        {
            return TypedResults.Conflict($"Row for ({request.BusinessEntityId}, {request.StartDate:O}, {request.TerritoryId}) already exists.");
        }

        var entity = request.ToEntity();
        db.SalesTerritoryHistories.Add(entity);
        await db.SaveChangesAsync(ct);
        db.SalesTerritoryHistoryAuditLogs.Add(SalesTerritoryHistoryAuditService.RecordCreate(entity, user.Identity?.Name));
        await db.SaveChangesAsync(ct);
        return TypedResults.Created(
            $"/api/aw/sales-territory-histories/by-key?businessEntityId={entity.BusinessEntityId}&startDate={entity.StartDate:O}&territoryId={entity.TerritoryId}",
            new CompositeKeyResponse(new Dictionary<string, object>
            {
                ["businessEntityId"] = entity.BusinessEntityId,
                ["startDate"] = entity.StartDate,
                ["territoryId"] = entity.TerritoryId,
            }));
    }

    private static async Task<Results<Ok<CompositeKeyResponse>, NotFound, ValidationProblem>> UpdateAsync(
        [FromQuery] int businessEntityId, [FromQuery] DateTime startDate, [FromQuery] int territoryId,
        UpdateSalesTerritoryHistoryRequest request, IValidator<UpdateSalesTerritoryHistoryRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());

        var entity = await db.SalesTerritoryHistories
            .FirstOrDefaultAsync(x => x.BusinessEntityId == businessEntityId && x.StartDate == startDate && x.TerritoryId == territoryId, ct);
        if (entity is null) return TypedResults.NotFound();

        var before = SalesTerritoryHistoryAuditService.CaptureSnapshot(entity);
        request.ApplyTo(entity);
        db.SalesTerritoryHistoryAuditLogs.Add(
            SalesTerritoryHistoryAuditService.RecordUpdate(before, entity, user.Identity?.Name));
        await db.SaveChangesAsync(ct);
        return TypedResults.Ok(new CompositeKeyResponse(new Dictionary<string, object>
        {
            ["businessEntityId"] = entity.BusinessEntityId,
            ["startDate"] = entity.StartDate,
            ["territoryId"] = entity.TerritoryId,
        }));
    }

    private static async Task<Results<NoContent, NotFound>> DeleteAsync(
        [FromQuery] int businessEntityId, [FromQuery] DateTime startDate, [FromQuery] int territoryId,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var entity = await db.SalesTerritoryHistories
            .FirstOrDefaultAsync(x => x.BusinessEntityId == businessEntityId && x.StartDate == startDate && x.TerritoryId == territoryId, ct);
        if (entity is null) return TypedResults.NotFound();

        db.SalesTerritoryHistoryAuditLogs.Add(SalesTerritoryHistoryAuditService.RecordDelete(entity, user.Identity?.Name));
        db.SalesTerritoryHistories.Remove(entity);
        await db.SaveChangesAsync(ct);
        return TypedResults.NoContent();
    }

    private static async Task<Ok<List<SalesTerritoryHistoryAuditLogDto>>> HistoryAsync(
        ApplicationDbContext db,
        [FromQuery] int? businessEntityId = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] int? territoryId = null,
        CancellationToken ct = default)
    {
        var query = db.SalesTerritoryHistoryAuditLogs.AsNoTracking();
        if (businessEntityId.HasValue) query = query.Where(a => a.BusinessEntityId == businessEntityId.Value);
        if (startDate.HasValue) query = query.Where(a => a.StartDate == startDate.Value);
        if (territoryId.HasValue) query = query.Where(a => a.TerritoryId == territoryId.Value);

        var rows = await query
            .OrderByDescending(a => a.ChangedDate).ThenByDescending(a => a.Id)
            .Select(a => a.ToDto())
            .ToListAsync(ct);
        return TypedResults.Ok(rows);
    }
}
