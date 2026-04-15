using System.Security.Claims;
using AWBlazorApp.Data;
using AWBlazorApp.Infrastructure.Persistence;
using AWBlazorApp.Shared.Models;
using AWBlazorApp.Features.AdventureWorks.Models;
using AWBlazorApp.Features.AdventureWorks.Audit;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.AdventureWorks.Endpoints;

public static class EmployeeDepartmentHistoryEndpoints
{
    public static IEndpointRouteBuilder MapEmployeeDepartmentHistoryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/aw/employee-department-histories")
            .WithTags("EmployeeDepartmentHistories")
            .RequireAuthorization("ApiOrCookie");

        group.MapGet("/", ListAsync).WithName("ListEmployeeDepartmentHistories")
            .WithSummary("List HumanResources.EmployeeDepartmentHistory rows. 4-column composite PK.");
        group.MapGet("/by-key", GetAsync).WithName("GetEmployeeDepartmentHistory");
        group.MapPost("/", CreateAsync).WithName("CreateEmployeeDepartmentHistory")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));
        group.MapPatch("/by-key", UpdateAsync).WithName("UpdateEmployeeDepartmentHistory")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));
        group.MapDelete("/by-key", DeleteAsync).WithName("DeleteEmployeeDepartmentHistory")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));
        group.MapGet("/history", HistoryAsync).WithName("ListEmployeeDepartmentHistoryHistory");
        return app;
    }

    private static async Task<Ok<PagedResult<EmployeeDepartmentHistoryDto>>> ListAsync(
        ApplicationDbContext db, [FromQuery] int skip = 0, [FromQuery] int take = 50,
        [FromQuery] int? businessEntityId = null, [FromQuery] short? departmentId = null,
        CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 1000);
        var query = db.EmployeeDepartmentHistories.AsNoTracking();
        if (businessEntityId.HasValue) query = query.Where(x => x.BusinessEntityId == businessEntityId.Value);
        if (departmentId.HasValue) query = query.Where(x => x.DepartmentId == departmentId.Value);
        var total = await query.CountAsync(ct);
        var rows = await query
            .OrderBy(x => x.BusinessEntityId).ThenBy(x => x.DepartmentId)
            .ThenBy(x => x.ShiftId).ThenByDescending(x => x.StartDate)
            .Skip(skip).Take(take).Select(x => x.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<EmployeeDepartmentHistoryDto>(rows, total, skip, take));
    }

    private static async Task<Results<Ok<EmployeeDepartmentHistoryDto>, NotFound>> GetAsync(
        [FromQuery] int businessEntityId, [FromQuery] short departmentId,
        [FromQuery] byte shiftId, [FromQuery] DateTime startDate,
        ApplicationDbContext db, CancellationToken ct)
    {
        var key = startDate.Date;
        var row = await db.EmployeeDepartmentHistories.AsNoTracking()
            .FirstOrDefaultAsync(x => x.BusinessEntityId == businessEntityId
                && x.DepartmentId == departmentId
                && x.ShiftId == shiftId
                && x.StartDate == key, ct);
        return row is null ? TypedResults.NotFound() : TypedResults.Ok(row.ToDto());
    }

    private static async Task<Results<Created<CompositeKeyResponse>, Conflict<string>, ValidationProblem>> CreateAsync(
        CreateEmployeeDepartmentHistoryRequest request,
        IValidator<CreateEmployeeDepartmentHistoryRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());

        var key = request.StartDate.Date;
        if (await db.EmployeeDepartmentHistories.AnyAsync(x =>
                x.BusinessEntityId == request.BusinessEntityId
                && x.DepartmentId == request.DepartmentId
                && x.ShiftId == request.ShiftId
                && x.StartDate == key, ct))
        {
            return TypedResults.Conflict(
                $"Row ({request.BusinessEntityId}, {request.DepartmentId}, {request.ShiftId}, {request.StartDate:yyyy-MM-dd}) already exists.");
        }

        var entity = request.ToEntity();
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        db.EmployeeDepartmentHistories.Add(entity);
        await db.SaveChangesAsync(ct);
        db.EmployeeDepartmentHistoryAuditLogs.Add(
            EmployeeDepartmentHistoryAuditService.RecordCreate(entity, user.Identity?.Name));
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return TypedResults.Created(
            $"/api/aw/employee-department-histories/by-key?businessEntityId={entity.BusinessEntityId}&departmentId={entity.DepartmentId}&shiftId={entity.ShiftId}&startDate={entity.StartDate:yyyy-MM-dd}",
            new CompositeKeyResponse(new Dictionary<string, object>
            {
                ["businessEntityId"] = entity.BusinessEntityId,
                ["departmentId"] = entity.DepartmentId,
                ["shiftId"] = entity.ShiftId,
                ["startDate"] = entity.StartDate,
            }));
    }

    private static async Task<Results<Ok<CompositeKeyResponse>, NotFound, ValidationProblem>> UpdateAsync(
        [FromQuery] int businessEntityId, [FromQuery] short departmentId,
        [FromQuery] byte shiftId, [FromQuery] DateTime startDate,
        UpdateEmployeeDepartmentHistoryRequest request,
        IValidator<UpdateEmployeeDepartmentHistoryRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());

        var key = startDate.Date;
        var entity = await db.EmployeeDepartmentHistories
            .FirstOrDefaultAsync(x => x.BusinessEntityId == businessEntityId
                && x.DepartmentId == departmentId
                && x.ShiftId == shiftId
                && x.StartDate == key, ct);
        if (entity is null) return TypedResults.NotFound();

        var before = EmployeeDepartmentHistoryAuditService.CaptureSnapshot(entity);
        request.ApplyTo(entity);
        db.EmployeeDepartmentHistoryAuditLogs.Add(
            EmployeeDepartmentHistoryAuditService.RecordUpdate(before, entity, user.Identity?.Name));
        await db.SaveChangesAsync(ct);
        return TypedResults.Ok(new CompositeKeyResponse(new Dictionary<string, object>
        {
            ["businessEntityId"] = entity.BusinessEntityId,
            ["departmentId"] = entity.DepartmentId,
            ["shiftId"] = entity.ShiftId,
            ["startDate"] = entity.StartDate,
        }));
    }

    private static async Task<Results<NoContent, NotFound>> DeleteAsync(
        [FromQuery] int businessEntityId, [FromQuery] short departmentId,
        [FromQuery] byte shiftId, [FromQuery] DateTime startDate,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var key = startDate.Date;
        var entity = await db.EmployeeDepartmentHistories
            .FirstOrDefaultAsync(x => x.BusinessEntityId == businessEntityId
                && x.DepartmentId == departmentId
                && x.ShiftId == shiftId
                && x.StartDate == key, ct);
        if (entity is null) return TypedResults.NotFound();

        db.EmployeeDepartmentHistoryAuditLogs.Add(
            EmployeeDepartmentHistoryAuditService.RecordDelete(entity, user.Identity?.Name));
        db.EmployeeDepartmentHistories.Remove(entity);
        await db.SaveChangesAsync(ct);
        return TypedResults.NoContent();
    }

    private static async Task<Ok<List<EmployeeDepartmentHistoryAuditLogDto>>> HistoryAsync(
        ApplicationDbContext db,
        [FromQuery] int? businessEntityId = null,
        [FromQuery] short? departmentId = null,
        [FromQuery] byte? shiftId = null,
        [FromQuery] DateTime? startDate = null,
        CancellationToken ct = default)
    {
        var query = db.EmployeeDepartmentHistoryAuditLogs.AsNoTracking();
        if (businessEntityId.HasValue) query = query.Where(a => a.BusinessEntityId == businessEntityId.Value);
        if (departmentId.HasValue) query = query.Where(a => a.DepartmentId == departmentId.Value);
        if (shiftId.HasValue) query = query.Where(a => a.ShiftId == shiftId.Value);
        if (startDate.HasValue)
        {
            var key = startDate.Value.Date;
            query = query.Where(a => a.StartDate == key);
        }

        var rows = await query
            .OrderByDescending(a => a.ChangedDate).ThenByDescending(a => a.Id)
            .Select(a => a.ToDto())
            .ToListAsync(ct);
        return TypedResults.Ok(rows);
    }
}
