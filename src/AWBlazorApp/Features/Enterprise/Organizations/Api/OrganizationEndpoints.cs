using AWBlazorApp.Features.Identity.Domain; using AWBlazorApp.Features.Admin.Permissions.Domain;
using AWBlazorApp.Infrastructure.Persistence;
using AWBlazorApp.Shared.Api;
using AWBlazorApp.Shared.Dtos;
using AWBlazorApp.Features.Enterprise.Assets.Application.Services; using AWBlazorApp.Features.Enterprise.OrgUnits.Application.Services; using AWBlazorApp.Features.Enterprise.Organizations.Application.Services; using AWBlazorApp.Features.Enterprise.ProductLines.Application.Services; using AWBlazorApp.Features.Enterprise.Stations.Application.Services; 
using AWBlazorApp.Features.Enterprise.Assets.Domain; using AWBlazorApp.Features.Enterprise.CostCenters.Domain; using AWBlazorApp.Features.Enterprise.OrgUnits.Domain; using AWBlazorApp.Features.Enterprise.Organizations.Domain; using AWBlazorApp.Features.Enterprise.ProductLines.Domain; using AWBlazorApp.Features.Enterprise.Stations.Domain; 
using AWBlazorApp.Features.Enterprise.Assets.Dtos; using AWBlazorApp.Features.Enterprise.CostCenters.Dtos; using AWBlazorApp.Features.Enterprise.OrgUnits.Dtos; using AWBlazorApp.Features.Enterprise.Organizations.Dtos; using AWBlazorApp.Features.Enterprise.ProductLines.Dtos; using AWBlazorApp.Features.Enterprise.Stations.Dtos; 
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.Enterprise.Organizations.Api;

public static class OrganizationEndpoints
{
    public static IEndpointRouteBuilder MapOrganizationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/organizations")
            .WithTags("Organizations")
            .RequireAuthorization("ApiOrCookie");

        group.MapGet("/", ListAsync).WithName("ListOrganizations").WithSummary("List org.Organization rows.");

        group.MapIntIdCrud<Organization, OrganizationDto, CreateOrganizationRequest, UpdateOrganizationRequest, OrganizationAuditLog, OrganizationAuditLogDto, OrganizationAuditService.Snapshot, int>(
            entityName: "Organization",
            routePrefix: "/api/organizations",
            entitySet: db => db.Organizations,
            auditSet: db => db.OrganizationAuditLogs,
            idSelector: e => e.Id,
            auditIdSelector: a => a.OrganizationId,
            auditChangedDateSelector: a => a.ChangedDate,
            auditPrimaryKeySelector: a => a.Id,
            getId: e => e.Id,
            toDto: e => e.ToDto(),
            toEntity: r => r.ToEntity(),
            applyUpdate: (r, e) => r.ApplyTo(e),
            captureSnapshot: OrganizationAuditService.CaptureSnapshot,
            recordCreate: OrganizationAuditService.RecordCreate,
            recordUpdate: OrganizationAuditService.RecordUpdate,
            recordDelete: OrganizationAuditService.RecordDelete,
            auditToDto: a => a.ToDto());

        return app;
    }

    private static async Task<Ok<PagedResult<OrganizationDto>>> ListAsync(
        ApplicationDbContext db,
        [FromQuery] int skip = 0, [FromQuery] int take = 50,
        [FromQuery] string? code = null, [FromQuery] string? name = null,
        [FromQuery] bool? isPrimary = null, [FromQuery] bool? isActive = null,
        CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 1000);
        var query = db.Organizations.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(code)) query = query.Where(x => x.Code.Contains(code));
        if (!string.IsNullOrWhiteSpace(name)) query = query.Where(x => x.Name.Contains(name));
        if (isPrimary.HasValue) query = query.Where(x => x.IsPrimary == isPrimary.Value);
        if (isActive.HasValue) query = query.Where(x => x.IsActive == isActive.Value);
        var total = await query.CountAsync(ct);
        var rows = await query.OrderBy(x => x.Code).Skip(skip).Take(take).Select(x => x.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<OrganizationDto>(rows, total, skip, take));
    }
}
