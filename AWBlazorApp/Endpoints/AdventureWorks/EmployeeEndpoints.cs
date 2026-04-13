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

public static class EmployeeEndpoints
{
    public static IEndpointRouteBuilder MapEmployeeEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/aw/employees")
            .WithTags("Employees")
            .RequireAuthorization("ApiOrCookie");

        group.MapGet("/", ListAsync).WithName("ListEmployees").WithSummary("List HumanResources.Employee rows.");
        group.MapGet("/{id:int}", GetAsync).WithName("GetEmployee");
        group.MapPost("/", CreateAsync).WithName("CreateEmployee")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));
        group.MapPatch("/{id:int}", UpdateAsync).WithName("UpdateEmployee")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));
        group.MapDelete("/{id:int}", DeleteAsync).WithName("DeleteEmployee")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));
        group.MapGet("/{id:int}/history", HistoryAsync).WithName("ListEmployeeHistory");
        return app;
    }

    private static async Task<Ok<PagedResult<EmployeeDto>>> ListAsync(
        ApplicationDbContext db, [FromQuery] int skip = 0, [FromQuery] int take = 50,
        [FromQuery] string? jobTitle = null, [FromQuery] string? loginId = null, CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 1000);
        var query = db.Employees.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(jobTitle)) query = query.Where(x => x.JobTitle.Contains(jobTitle));
        if (!string.IsNullOrWhiteSpace(loginId)) query = query.Where(x => x.LoginID.Contains(loginId));
        var total = await query.CountAsync(ct);
        var rows = await query.OrderBy(x => x.Id).Skip(skip).Take(take).Select(x => x.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<EmployeeDto>(rows, total, skip, take));
    }

    private static async Task<Results<Ok<EmployeeDto>, NotFound>> GetAsync(int id, ApplicationDbContext db, CancellationToken ct)
    {
        var row = await db.Employees.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        return row is null ? TypedResults.NotFound() : TypedResults.Ok(row.ToDto());
    }

    private static async Task<Results<Created<IdResponse>, Conflict<string>, ValidationProblem>> CreateAsync(
        CreateEmployeeRequest request, IValidator<CreateEmployeeRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());

        if (await db.Employees.AnyAsync(x => x.Id == request.Id, ct))
            return TypedResults.Conflict($"Employee with BusinessEntityId {request.Id} already exists.");

        var entity = request.ToEntity();
        db.Employees.Add(entity);
        await db.SaveChangesAsync(ct);
        db.EmployeeAuditLogs.Add(EmployeeAuditService.RecordCreate(entity, user.Identity?.Name));
        await db.SaveChangesAsync(ct);
        return TypedResults.Created($"/api/aw/employees/{entity.Id}", new IdResponse(entity.Id));
    }

    private static async Task<Results<Ok<IdResponse>, NotFound, ValidationProblem>> UpdateAsync(
        int id, UpdateEmployeeRequest request, IValidator<UpdateEmployeeRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());

        var entity = await db.Employees.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null) return TypedResults.NotFound();

        var before = EmployeeAuditService.CaptureSnapshot(entity);
        request.ApplyTo(entity);
        db.EmployeeAuditLogs.Add(EmployeeAuditService.RecordUpdate(before, entity, user.Identity?.Name));
        await db.SaveChangesAsync(ct);
        return TypedResults.Ok(new IdResponse(entity.Id));
    }

    private static async Task<Results<NoContent, NotFound>> DeleteAsync(
        int id, ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var entity = await db.Employees.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null) return TypedResults.NotFound();

        db.EmployeeAuditLogs.Add(EmployeeAuditService.RecordDelete(entity, user.Identity?.Name));
        db.Employees.Remove(entity);
        await db.SaveChangesAsync(ct);
        return TypedResults.NoContent();
    }

    private static async Task<Ok<List<EmployeeAuditLogDto>>> HistoryAsync(int id, ApplicationDbContext db, CancellationToken ct)
    {
        var rows = await db.EmployeeAuditLogs.AsNoTracking()
            .Where(a => a.EmployeeId == id)
            .OrderByDescending(a => a.ChangedDate).ThenByDescending(a => a.Id)
            .Select(a => a.ToDto())
            .ToListAsync(ct);
        return TypedResults.Ok(rows);
    }
}
