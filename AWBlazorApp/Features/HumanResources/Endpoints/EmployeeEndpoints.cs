using AWBlazorApp.Data;
using AWBlazorApp.Infrastructure.Persistence;
using AWBlazorApp.Shared.Endpoints;
using AWBlazorApp.Shared.Models;
using AWBlazorApp.Features.HumanResources.Audit;
using AWBlazorApp.Features.HumanResources.Domain;
using AWBlazorApp.Features.HumanResources.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.HumanResources.Endpoints;

public static class EmployeeEndpoints
{
    public static IEndpointRouteBuilder MapEmployeeEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/aw/employees")
            .WithTags("Employees")
            .RequireAuthorization("ApiOrCookie");

        group.MapGet("/", ListAsync).WithName("ListEmployees").WithSummary("List HumanResources.Employee rows.");

        group.MapIntIdCrud<Employee, EmployeeDto, CreateEmployeeRequest, UpdateEmployeeRequest, EmployeeAuditLog, EmployeeAuditLogDto, EmployeeAuditService.Snapshot, int>(
            entityName: "Employee",
            routePrefix: "/api/aw/employees",
            entitySet: db => db.Employees,
            auditSet: db => db.EmployeeAuditLogs,
            idSelector: e => e.Id,
            auditIdSelector: a => a.EmployeeId,
            auditChangedDateSelector: a => a.ChangedDate,
            auditPrimaryKeySelector: a => a.Id,
            getId: e => e.Id,
            toDto: e => e.ToDto(),
            toEntity: r => r.ToEntity(),
            applyUpdate: (r, e) => r.ApplyTo(e),
            captureSnapshot: EmployeeAuditService.CaptureSnapshot,
            recordCreate: EmployeeAuditService.RecordCreate,
            recordUpdate: EmployeeAuditService.RecordUpdate,
            recordDelete: EmployeeAuditService.RecordDelete,
            auditToDto: a => a.ToDto());

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
}