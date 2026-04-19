using AWBlazorApp.Features.Identity.Domain; using AWBlazorApp.Features.Admin.Permissions.Domain;
using AWBlazorApp.Infrastructure.Persistence;
using AWBlazorApp.Shared.Api;
using AWBlazorApp.Shared.Dtos;
using AWBlazorApp.Features.Production.Audit;
using AWBlazorApp.Features.Production.Domain;
using AWBlazorApp.Features.Production.Dtos;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.Production.Api;

public static class BillOfMaterialsEndpoints
{
    public static IEndpointRouteBuilder MapBillOfMaterialsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/aw/bill-of-materials")
            .WithTags("BillOfMaterials")
            .RequireAuthorization("ApiOrCookie");

        group.MapGet("/", ListAsync).WithName("ListBillOfMaterials").WithSummary("List Production.BillOfMaterials rows.");

        group.MapIntIdCrud<BillOfMaterials, BillOfMaterialsDto, CreateBillOfMaterialsRequest, UpdateBillOfMaterialsRequest, BillOfMaterialsAuditLog, BillOfMaterialsAuditLogDto, BillOfMaterialsAuditService.Snapshot, int>(
            entityName: "BillOfMaterials",
            routePrefix: "/api/aw/bill-of-materials",
            entitySet: db => db.BillOfMaterials,
            auditSet: db => db.BillOfMaterialsAuditLogs,
            idSelector: e => e.Id,
            auditIdSelector: a => a.BillOfMaterialsId,
            auditChangedDateSelector: a => a.ChangedDate,
            auditPrimaryKeySelector: a => a.Id,
            getId: e => e.Id,
            toDto: e => e.ToDto(),
            toEntity: r => r.ToEntity(),
            applyUpdate: (r, e) => r.ApplyTo(e),
            captureSnapshot: BillOfMaterialsAuditService.CaptureSnapshot,
            recordCreate: BillOfMaterialsAuditService.RecordCreate,
            recordUpdate: BillOfMaterialsAuditService.RecordUpdate,
            recordDelete: BillOfMaterialsAuditService.RecordDelete,
            auditToDto: a => a.ToDto());

        return app;
    }

    private static async Task<Ok<PagedResult<BillOfMaterialsDto>>> ListAsync(
        ApplicationDbContext db, [FromQuery] int skip = 0, [FromQuery] int take = 50,
        [FromQuery] int? componentId = null, CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 1000);
        var query = db.BillOfMaterials.AsNoTracking();
        if (componentId.HasValue) query = query.Where(x => x.ComponentId == componentId.Value);
        var total = await query.CountAsync(ct);
        var rows = await query.OrderBy(x => x.Id).Skip(skip).Take(take).Select(x => x.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<BillOfMaterialsDto>(rows, total, skip, take));
    }
}