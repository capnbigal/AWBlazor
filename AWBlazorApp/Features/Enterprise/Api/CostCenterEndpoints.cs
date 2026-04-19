using AWBlazorApp.Features.Identity.Domain; using AWBlazorApp.Features.Admin.Permissions.Domain;
using AWBlazorApp.Infrastructure.Persistence;
using AWBlazorApp.Shared.Api;
using AWBlazorApp.Shared.Dtos;
using AWBlazorApp.Features.Enterprise.Audit;
using AWBlazorApp.Features.Enterprise.Domain;
using AWBlazorApp.Features.Enterprise.Dtos;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.Enterprise.Api;

public static class CostCenterEndpoints
{
    public static IEndpointRouteBuilder MapCostCenterEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/cost-centers")
            .WithTags("CostCenters")
            .RequireAuthorization("ApiOrCookie");

        group.MapGet("/", ListAsync).WithName("ListCostCenters").WithSummary("List org.CostCenter rows.");

        group.MapIntIdCrud<CostCenter, CostCenterDto, CreateCostCenterRequest, UpdateCostCenterRequest, CostCenterAuditLog, CostCenterAuditLogDto, CostCenterAuditService.Snapshot, int>(
            entityName: "CostCenter",
            routePrefix: "/api/cost-centers",
            entitySet: db => db.CostCenters,
            auditSet: db => db.CostCenterAuditLogs,
            idSelector: e => e.Id,
            auditIdSelector: a => a.CostCenterId,
            auditChangedDateSelector: a => a.ChangedDate,
            auditPrimaryKeySelector: a => a.Id,
            getId: e => e.Id,
            toDto: e => e.ToDto(),
            toEntity: r => r.ToEntity(),
            applyUpdate: (r, e) => r.ApplyTo(e),
            captureSnapshot: CostCenterAuditService.CaptureSnapshot,
            recordCreate: CostCenterAuditService.RecordCreate,
            recordUpdate: CostCenterAuditService.RecordUpdate,
            recordDelete: CostCenterAuditService.RecordDelete,
            auditToDto: a => a.ToDto());

        return app;
    }

    private static async Task<Ok<PagedResult<CostCenterDto>>> ListAsync(
        ApplicationDbContext db,
        [FromQuery] int skip = 0, [FromQuery] int take = 50,
        [FromQuery] int? organizationId = null, [FromQuery] string? code = null,
        [FromQuery] string? name = null, [FromQuery] bool? isActive = null,
        CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 1000);
        var query = db.CostCenters.AsNoTracking();
        if (organizationId.HasValue) query = query.Where(x => x.OrganizationId == organizationId.Value);
        if (!string.IsNullOrWhiteSpace(code)) query = query.Where(x => x.Code.Contains(code));
        if (!string.IsNullOrWhiteSpace(name)) query = query.Where(x => x.Name.Contains(name));
        if (isActive.HasValue) query = query.Where(x => x.IsActive == isActive.Value);
        var total = await query.CountAsync(ct);
        var rows = await query.OrderBy(x => x.Code)
            .Skip(skip).Take(take).Select(x => x.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<CostCenterDto>(rows, total, skip, take));
    }
}
