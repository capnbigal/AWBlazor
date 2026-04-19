using AWBlazorApp.Features.Identity.Domain; using AWBlazorApp.Features.Admin.Permissions.Domain;
using AWBlazorApp.Infrastructure.Persistence;
using AWBlazorApp.Shared.Api;
using AWBlazorApp.Shared.Dtos;
using AWBlazorApp.Features.HumanResources.Departments.Application.Services; using AWBlazorApp.Features.HumanResources.Employees.Application.Services; using AWBlazorApp.Features.HumanResources.EmployeeDepartmentHistories.Application.Services; using AWBlazorApp.Features.HumanResources.EmployeePayHistories.Application.Services; using AWBlazorApp.Features.HumanResources.JobCandidates.Application.Services; using AWBlazorApp.Features.HumanResources.Shifts.Application.Services; 
using AWBlazorApp.Features.HumanResources.Departments.Domain; using AWBlazorApp.Features.HumanResources.Employees.Domain; using AWBlazorApp.Features.HumanResources.EmployeeDepartmentHistories.Domain; using AWBlazorApp.Features.HumanResources.EmployeePayHistories.Domain; using AWBlazorApp.Features.HumanResources.JobCandidates.Domain; using AWBlazorApp.Features.HumanResources.Shifts.Domain; 
using AWBlazorApp.Features.HumanResources.Departments.Dtos; using AWBlazorApp.Features.HumanResources.Employees.Dtos; using AWBlazorApp.Features.HumanResources.EmployeeDepartmentHistories.Dtos; using AWBlazorApp.Features.HumanResources.EmployeePayHistories.Dtos; using AWBlazorApp.Features.HumanResources.JobCandidates.Dtos; using AWBlazorApp.Features.HumanResources.Shifts.Dtos; 
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.HumanResources.Departments.Api;

public static class DepartmentEndpoints
{
    public static IEndpointRouteBuilder MapDepartmentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/aw/departments")
            .WithTags("Departments")
            .RequireAuthorization("ApiOrCookie");

        group.MapGet("/", ListAsync).WithName("ListDepartments").WithSummary("List HumanResources.Department rows.");

        group.MapIntIdCrud<Department, DepartmentDto, CreateDepartmentRequest, UpdateDepartmentRequest, DepartmentAuditLog, DepartmentAuditLogDto, DepartmentAuditService.Snapshot, short>(
            entityName: "Department",
            routePrefix: "/api/aw/departments",
            entitySet: db => db.Departments,
            auditSet: db => db.DepartmentAuditLogs,
            idSelector: e => e.Id,
            auditIdSelector: a => a.DepartmentId,
            auditChangedDateSelector: a => a.ChangedDate,
            auditPrimaryKeySelector: a => a.Id,
            getId: e => e.Id,
            toDto: e => e.ToDto(),
            toEntity: r => r.ToEntity(),
            applyUpdate: (r, e) => r.ApplyTo(e),
            captureSnapshot: DepartmentAuditService.CaptureSnapshot,
            recordCreate: DepartmentAuditService.RecordCreate,
            recordUpdate: DepartmentAuditService.RecordUpdate,
            recordDelete: DepartmentAuditService.RecordDelete,
            auditToDto: a => a.ToDto());

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
}