using System.Security.Claims;
using AWBlazorApp.Data;
using AWBlazorApp.Infrastructure.Persistence;
using AWBlazorApp.Shared.Models;
using AWBlazorApp.Features.Sales.Models;
using AWBlazorApp.Features.Sales.Audit;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.Sales.Endpoints;

public static class SalesPersonQuotaHistoryEndpoints
{
    public static IEndpointRouteBuilder MapSalesPersonQuotaHistoryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/aw/sales-person-quota-histories")
            .WithTags("SalesPersonQuotaHistories")
            .RequireAuthorization("ApiOrCookie");

        group.MapGet("/", ListAsync).WithName("ListSalesPersonQuotaHistories")
            .WithSummary("List Sales.SalesPersonQuotaHistory rows. Composite PK = (BusinessEntityID, QuotaDate).");
        group.MapGet("/by-key", GetAsync).WithName("GetSalesPersonQuotaHistory");
        group.MapPost("/", CreateAsync).WithName("CreateSalesPersonQuotaHistory")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));
        group.MapPatch("/by-key", UpdateAsync).WithName("UpdateSalesPersonQuotaHistory")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));
        group.MapDelete("/by-key", DeleteAsync).WithName("DeleteSalesPersonQuotaHistory")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));
        group.MapGet("/history", HistoryAsync).WithName("ListSalesPersonQuotaHistoryHistory");
        return app;
    }

    private static async Task<Ok<PagedResult<SalesPersonQuotaHistoryDto>>> ListAsync(
        ApplicationDbContext db, [FromQuery] int skip = 0, [FromQuery] int take = 50,
        [FromQuery] int? businessEntityId = null, CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 1000);
        var query = db.SalesPersonQuotaHistories.AsNoTracking();
        if (businessEntityId.HasValue) query = query.Where(x => x.BusinessEntityId == businessEntityId.Value);
        var total = await query.CountAsync(ct);
        var rows = await query.OrderBy(x => x.BusinessEntityId).ThenByDescending(x => x.QuotaDate)
            .Skip(skip).Take(take).Select(x => x.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<SalesPersonQuotaHistoryDto>(rows, total, skip, take));
    }

    private static async Task<Results<Ok<SalesPersonQuotaHistoryDto>, NotFound>> GetAsync(
        [FromQuery] int businessEntityId, [FromQuery] DateTime quotaDate,
        ApplicationDbContext db, CancellationToken ct)
    {
        var row = await db.SalesPersonQuotaHistories.AsNoTracking()
            .FirstOrDefaultAsync(x => x.BusinessEntityId == businessEntityId && x.QuotaDate == quotaDate, ct);
        return row is null ? TypedResults.NotFound() : TypedResults.Ok(row.ToDto());
    }

    private static async Task<Results<Created<CompositeKeyResponse>, Conflict<string>, ValidationProblem>> CreateAsync(
        CreateSalesPersonQuotaHistoryRequest request, IValidator<CreateSalesPersonQuotaHistoryRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());

        if (await db.SalesPersonQuotaHistories.AnyAsync(x =>
                x.BusinessEntityId == request.BusinessEntityId && x.QuotaDate == request.QuotaDate, ct))
        {
            return TypedResults.Conflict($"Quota row for ({request.BusinessEntityId}, {request.QuotaDate:O}) already exists.");
        }

        var entity = request.ToEntity();
        await db.AddWithAuditAsync(entity, e => SalesPersonQuotaHistoryAuditService.RecordCreate(e, user.Identity?.Name), ct);
        return TypedResults.Created(
            $"/api/aw/sales-person-quota-histories/by-key?businessEntityId={entity.BusinessEntityId}&quotaDate={entity.QuotaDate:O}",
            new CompositeKeyResponse(new Dictionary<string, object>
            {
                ["businessEntityId"] = entity.BusinessEntityId,
                ["quotaDate"] = entity.QuotaDate,
            }));
    }

    private static async Task<Results<Ok<CompositeKeyResponse>, NotFound, ValidationProblem>> UpdateAsync(
        [FromQuery] int businessEntityId, [FromQuery] DateTime quotaDate,
        UpdateSalesPersonQuotaHistoryRequest request, IValidator<UpdateSalesPersonQuotaHistoryRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());

        var entity = await db.SalesPersonQuotaHistories
            .FirstOrDefaultAsync(x => x.BusinessEntityId == businessEntityId && x.QuotaDate == quotaDate, ct);
        if (entity is null) return TypedResults.NotFound();

        var before = SalesPersonQuotaHistoryAuditService.CaptureSnapshot(entity);
        request.ApplyTo(entity);
        db.SalesPersonQuotaHistoryAuditLogs.Add(
            SalesPersonQuotaHistoryAuditService.RecordUpdate(before, entity, user.Identity?.Name));
        await db.SaveChangesAsync(ct);
        return TypedResults.Ok(new CompositeKeyResponse(new Dictionary<string, object>
        {
            ["businessEntityId"] = entity.BusinessEntityId,
            ["quotaDate"] = entity.QuotaDate,
        }));
    }

    private static async Task<Results<NoContent, NotFound>> DeleteAsync(
        [FromQuery] int businessEntityId, [FromQuery] DateTime quotaDate,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var entity = await db.SalesPersonQuotaHistories
            .FirstOrDefaultAsync(x => x.BusinessEntityId == businessEntityId && x.QuotaDate == quotaDate, ct);
        if (entity is null) return TypedResults.NotFound();

        db.SalesPersonQuotaHistoryAuditLogs.Add(
            SalesPersonQuotaHistoryAuditService.RecordDelete(entity, user.Identity?.Name));
        db.SalesPersonQuotaHistories.Remove(entity);
        await db.SaveChangesAsync(ct);
        return TypedResults.NoContent();
    }

    private static async Task<Ok<List<SalesPersonQuotaHistoryAuditLogDto>>> HistoryAsync(
        ApplicationDbContext db,
        [FromQuery] int? businessEntityId = null,
        [FromQuery] DateTime? quotaDate = null,
        CancellationToken ct = default)
    {
        var query = db.SalesPersonQuotaHistoryAuditLogs.AsNoTracking();
        if (businessEntityId.HasValue) query = query.Where(a => a.BusinessEntityId == businessEntityId.Value);
        if (quotaDate.HasValue) query = query.Where(a => a.QuotaDate == quotaDate.Value);

        var rows = await query
            .OrderByDescending(a => a.ChangedDate).ThenByDescending(a => a.Id)
            .Select(a => a.ToDto())
            .ToListAsync(ct);
        return TypedResults.Ok(rows);
    }
}
