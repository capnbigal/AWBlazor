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

public static class WorkOrderEndpoints
{
    public static IEndpointRouteBuilder MapWorkOrderEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/aw/work-orders")
            .WithTags("WorkOrders")
            .RequireAuthorization("ApiOrCookie");

        group.MapGet("/", ListAsync).WithName("ListWorkOrders").WithSummary("List Production.WorkOrder rows.");

        group.MapIntIdCrud<WorkOrder, WorkOrderDto, CreateWorkOrderRequest, UpdateWorkOrderRequest, WorkOrderAuditLog, WorkOrderAuditLogDto, WorkOrderAuditService.Snapshot, int>(
            entityName: "WorkOrder",
            routePrefix: "/api/aw/work-orders",
            entitySet: db => db.WorkOrders,
            auditSet: db => db.WorkOrderAuditLogs,
            idSelector: e => e.Id,
            auditIdSelector: a => a.WorkOrderId,
            auditChangedDateSelector: a => a.ChangedDate,
            auditPrimaryKeySelector: a => a.Id,
            getId: e => e.Id,
            toDto: e => e.ToDto(),
            toEntity: r => r.ToEntity(),
            applyUpdate: (r, e) => r.ApplyTo(e),
            captureSnapshot: WorkOrderAuditService.CaptureSnapshot,
            recordCreate: WorkOrderAuditService.RecordCreate,
            recordUpdate: WorkOrderAuditService.RecordUpdate,
            recordDelete: WorkOrderAuditService.RecordDelete,
            auditToDto: a => a.ToDto());

        return app;
    }

    private static async Task<Ok<PagedResult<WorkOrderDto>>> ListAsync(
        ApplicationDbContext db, [FromQuery] int skip = 0, [FromQuery] int take = 50,
        [FromQuery] int? productId = null, CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 1000);
        var query = db.WorkOrders.AsNoTracking();
        if (productId.HasValue) query = query.Where(x => x.ProductId == productId.Value);
        var total = await query.CountAsync(ct);
        var rows = await query.OrderBy(x => x.Id).Skip(skip).Take(take).Select(x => x.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<WorkOrderDto>(rows, total, skip, take));
    }
}