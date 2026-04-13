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

public static class DepartmentEndpoints
{
    public static IEndpointRouteBuilder MapDepartmentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/aw/departments")
            .WithTags("Departments")
            .RequireAuthorization("ApiOrCookie");

        group.MapGet("/", ListAsync).WithName("ListDepartments").WithSummary("List HumanResources.Department rows.");
        group.MapGet("/{id:int}", GetAsync).WithName("GetDepartment");
        group.MapPost("/", CreateAsync).WithName("CreateDepartment")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));
        group.MapPatch("/{id:int}", UpdateAsync).WithName("UpdateDepartment")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));
        group.MapDelete("/{id:int}", DeleteAsync).WithName("DeleteDepartment")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));
        group.MapGet("/{id:int}/history", HistoryAsync).WithName("ListDepartmentHistory");
        return app;
    }

    private static async Task<Ok<PagedResult<DepartmentDto>>> ListAsync(
        ApplicationDbContext db, [FromQuery] int skip = 0, [FromQuery] int take = 50,
        [FromQuery] string? name = null, [FromQuery] string? groupName = null, CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 1000);
        var query = db.Departments.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(name)) query = query.Where(x => x.Name.Contains(name));
        if (!string.IsNullOrWhiteSpace(groupName)) query = query.Where(x => x.GroupName == groupName);
        var total = await query.CountAsync(ct);
        var rows = await query.OrderBy(x => x.Id).Skip(skip).Take(take).Select(x => x.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<DepartmentDto>(rows, total, skip, take));
    }

    private static async Task<Results<Ok<DepartmentDto>, NotFound>> GetAsync(int id, ApplicationDbContext db, CancellationToken ct)
    {
        var key = (short)id;
        var row = await db.Departments.AsNoTracking().FirstOrDefaultAsync(x => x.Id == key, ct);
        return row is null ? TypedResults.NotFound() : TypedResults.Ok(row.ToDto());
    }

    private static async Task<Results<Created<IdResponse>, ValidationProblem>> CreateAsync(
        CreateDepartmentRequest request, IValidator<CreateDepartmentRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());

        var entity = request.ToEntity();
        db.Departments.Add(entity);
        await db.SaveChangesAsync(ct);
        db.DepartmentAuditLogs.Add(DepartmentAuditService.RecordCreate(entity, user.Identity?.Name));
        await db.SaveChangesAsync(ct);
        return TypedResults.Created($"/api/aw/departments/{entity.Id}", new IdResponse(entity.Id));
    }

    private static async Task<Results<Ok<IdResponse>, NotFound, ValidationProblem>> UpdateAsync(
        int id, UpdateDepartmentRequest request, IValidator<UpdateDepartmentRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());

        var key = (short)id;
        var entity = await db.Departments.FirstOrDefaultAsync(x => x.Id == key, ct);
        if (entity is null) return TypedResults.NotFound();

        var before = DepartmentAuditService.CaptureSnapshot(entity);
        request.ApplyTo(entity);
        db.DepartmentAuditLogs.Add(DepartmentAuditService.RecordUpdate(before, entity, user.Identity?.Name));
        await db.SaveChangesAsync(ct);
        return TypedResults.Ok(new IdResponse(entity.Id));
    }

    private static async Task<Results<NoContent, NotFound>> DeleteAsync(
        int id, ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var key = (short)id;
        var entity = await db.Departments.FirstOrDefaultAsync(x => x.Id == key, ct);
        if (entity is null) return TypedResults.NotFound();

        db.DepartmentAuditLogs.Add(DepartmentAuditService.RecordDelete(entity, user.Identity?.Name));
        db.Departments.Remove(entity);
        await db.SaveChangesAsync(ct);
        return TypedResults.NoContent();
    }

    private static async Task<Ok<List<DepartmentAuditLogDto>>> HistoryAsync(int id, ApplicationDbContext db, CancellationToken ct)
    {
        var key = (short)id;
        var rows = await db.DepartmentAuditLogs.AsNoTracking()
            .Where(a => a.DepartmentId == key)
            .OrderByDescending(a => a.ChangedDate).ThenByDescending(a => a.Id)
            .Select(a => a.ToDto())
            .ToListAsync(ct);
        return TypedResults.Ok(rows);
    }
}
