using System.Security.Claims;
using AWBlazorApp.Data;
using AWBlazorApp.Infrastructure.Persistence;
using AWBlazorApp.Models;
using AWBlazorApp.Models.AdventureWorks;
using AWBlazorApp.Services.AdventureWorksAudit;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Endpoints.AdventureWorks;

public static class EmployeePayHistoryEndpoints
{
    public static IEndpointRouteBuilder MapEmployeePayHistoryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/aw/employee-pay-histories")
            .WithTags("EmployeePayHistories")
            .RequireAuthorization("ApiOrCookie");

        group.MapGet("/", ListAsync).WithName("ListEmployeePayHistories")
            .WithSummary("List HumanResources.EmployeePayHistory rows. Composite PK = (BusinessEntityID, RateChangeDate).");
        group.MapGet("/by-key", GetAsync).WithName("GetEmployeePayHistory");
        group.MapPost("/", CreateAsync).WithName("CreateEmployeePayHistory")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));
        group.MapPatch("/by-key", UpdateAsync).WithName("UpdateEmployeePayHistory")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));
        group.MapDelete("/by-key", DeleteAsync).WithName("DeleteEmployeePayHistory")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));
        group.MapGet("/history", HistoryAsync).WithName("ListEmployeePayHistoryHistory");
        return app;
    }

    private static async Task<Ok<PagedResult<EmployeePayHistoryDto>>> ListAsync(
        ApplicationDbContext db, [FromQuery] int skip = 0, [FromQuery] int take = 50,
        [FromQuery] int? businessEntityId = null, CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 1000);
        var query = db.EmployeePayHistories.AsNoTracking();
        if (businessEntityId.HasValue) query = query.Where(x => x.BusinessEntityId == businessEntityId.Value);
        var total = await query.CountAsync(ct);
        var rows = await query.OrderBy(x => x.BusinessEntityId).ThenByDescending(x => x.RateChangeDate)
            .Skip(skip).Take(take).Select(x => x.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<EmployeePayHistoryDto>(rows, total, skip, take));
    }

    private static async Task<Results<Ok<EmployeePayHistoryDto>, NotFound>> GetAsync(
        [FromQuery] int businessEntityId, [FromQuery] DateTime rateChangeDate,
        ApplicationDbContext db, CancellationToken ct)
    {
        var row = await db.EmployeePayHistories.AsNoTracking()
            .FirstOrDefaultAsync(x => x.BusinessEntityId == businessEntityId && x.RateChangeDate == rateChangeDate, ct);
        return row is null ? TypedResults.NotFound() : TypedResults.Ok(row.ToDto());
    }

    private static async Task<Results<Created<CompositeKeyResponse>, Conflict<string>, ValidationProblem>> CreateAsync(
        CreateEmployeePayHistoryRequest request, IValidator<CreateEmployeePayHistoryRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());

        if (await db.EmployeePayHistories.AnyAsync(x =>
                x.BusinessEntityId == request.BusinessEntityId && x.RateChangeDate == request.RateChangeDate, ct))
        {
            return TypedResults.Conflict($"Pay history row for ({request.BusinessEntityId}, {request.RateChangeDate:O}) already exists.");
        }

        var entity = request.ToEntity();
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        db.EmployeePayHistories.Add(entity);
        await db.SaveChangesAsync(ct);
        db.EmployeePayHistoryAuditLogs.Add(EmployeePayHistoryAuditService.RecordCreate(entity, user.Identity?.Name));
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return TypedResults.Created(
            $"/api/aw/employee-pay-histories/by-key?businessEntityId={entity.BusinessEntityId}&rateChangeDate={entity.RateChangeDate:O}",
            new CompositeKeyResponse(new Dictionary<string, object>
            {
                ["businessEntityId"] = entity.BusinessEntityId,
                ["rateChangeDate"] = entity.RateChangeDate,
            }));
    }

    private static async Task<Results<Ok<CompositeKeyResponse>, NotFound, ValidationProblem>> UpdateAsync(
        [FromQuery] int businessEntityId, [FromQuery] DateTime rateChangeDate,
        UpdateEmployeePayHistoryRequest request, IValidator<UpdateEmployeePayHistoryRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());

        var entity = await db.EmployeePayHistories
            .FirstOrDefaultAsync(x => x.BusinessEntityId == businessEntityId && x.RateChangeDate == rateChangeDate, ct);
        if (entity is null) return TypedResults.NotFound();

        var before = EmployeePayHistoryAuditService.CaptureSnapshot(entity);
        request.ApplyTo(entity);
        db.EmployeePayHistoryAuditLogs.Add(
            EmployeePayHistoryAuditService.RecordUpdate(before, entity, user.Identity?.Name));
        await db.SaveChangesAsync(ct);
        return TypedResults.Ok(new CompositeKeyResponse(new Dictionary<string, object>
        {
            ["businessEntityId"] = entity.BusinessEntityId,
            ["rateChangeDate"] = entity.RateChangeDate,
        }));
    }

    private static async Task<Results<NoContent, NotFound>> DeleteAsync(
        [FromQuery] int businessEntityId, [FromQuery] DateTime rateChangeDate,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var entity = await db.EmployeePayHistories
            .FirstOrDefaultAsync(x => x.BusinessEntityId == businessEntityId && x.RateChangeDate == rateChangeDate, ct);
        if (entity is null) return TypedResults.NotFound();

        db.EmployeePayHistoryAuditLogs.Add(EmployeePayHistoryAuditService.RecordDelete(entity, user.Identity?.Name));
        db.EmployeePayHistories.Remove(entity);
        await db.SaveChangesAsync(ct);
        return TypedResults.NoContent();
    }

    private static async Task<Ok<List<EmployeePayHistoryAuditLogDto>>> HistoryAsync(
        ApplicationDbContext db,
        [FromQuery] int? businessEntityId = null,
        [FromQuery] DateTime? rateChangeDate = null,
        CancellationToken ct = default)
    {
        var query = db.EmployeePayHistoryAuditLogs.AsNoTracking();
        if (businessEntityId.HasValue) query = query.Where(a => a.BusinessEntityId == businessEntityId.Value);
        if (rateChangeDate.HasValue) query = query.Where(a => a.RateChangeDate == rateChangeDate.Value);

        var rows = await query
            .OrderByDescending(a => a.ChangedDate).ThenByDescending(a => a.Id)
            .Select(a => a.ToDto())
            .ToListAsync(ct);
        return TypedResults.Ok(rows);
    }
}
