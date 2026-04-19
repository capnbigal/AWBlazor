using AWBlazorApp.Features.Identity.Domain; using AWBlazorApp.Features.Admin.Permissions.Domain;
using AWBlazorApp.Infrastructure.Persistence;
using AWBlazorApp.Shared.Api;
using AWBlazorApp.Shared.Dtos;
using AWBlazorApp.Features.HumanResources.Audit;
using AWBlazorApp.Features.HumanResources.Domain;
using AWBlazorApp.Features.HumanResources.Dtos;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.HumanResources.Api;

public static class ShiftEndpoints
{
    public static IEndpointRouteBuilder MapShiftEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/aw/shifts")
            .WithTags("Shifts")
            .RequireAuthorization("ApiOrCookie");

        group.MapGet("/", ListAsync).WithName("ListShifts").WithSummary("List HumanResources.Shift rows.");

        group.MapIntIdCrud<Shift, ShiftDto, CreateShiftRequest, UpdateShiftRequest, ShiftAuditLog, ShiftAuditLogDto, ShiftAuditService.Snapshot, byte>(
            entityName: "Shift",
            routePrefix: "/api/aw/shifts",
            entitySet: db => db.Shifts,
            auditSet: db => db.ShiftAuditLogs,
            idSelector: e => e.Id,
            auditIdSelector: a => a.ShiftId,
            auditChangedDateSelector: a => a.ChangedDate,
            auditPrimaryKeySelector: a => a.Id,
            getId: e => e.Id,
            toDto: e => e.ToDto(),
            toEntity: r => r.ToEntity(),
            applyUpdate: (r, e) => r.ApplyTo(e),
            captureSnapshot: ShiftAuditService.CaptureSnapshot,
            recordCreate: ShiftAuditService.RecordCreate,
            recordUpdate: ShiftAuditService.RecordUpdate,
            recordDelete: ShiftAuditService.RecordDelete,
            auditToDto: a => a.ToDto());

        return app;
    }

    private static async Task<Ok<PagedResult<ShiftDto>>> ListAsync(
        ApplicationDbContext db, [FromQuery] int skip = 0, [FromQuery] int take = 50,
        [FromQuery] string? name = null, CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 1000);
        var query = db.Shifts.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(name)) query = query.Where(x => x.Name.Contains(name));
        var total = await query.CountAsync(ct);
        var rows = await query.OrderBy(x => x.Id).Skip(skip).Take(take).Select(x => x.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<ShiftDto>(rows, total, skip, take));
    }
}