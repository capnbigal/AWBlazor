using AWBlazorApp.Features.Identity.Domain; using AWBlazorApp.Features.Admin.Permissions.Domain;
using AWBlazorApp.Infrastructure.Persistence;
using AWBlazorApp.Shared.Api;
using AWBlazorApp.Shared.Dtos;
using AWBlazorApp.Features.Purchasing.Audit;
using AWBlazorApp.Features.Purchasing.Domain;
using AWBlazorApp.Features.Purchasing.Dtos;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.Purchasing.Api;

public static class VendorEndpoints
{
    public static IEndpointRouteBuilder MapVendorEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/aw/vendors")
            .WithTags("Vendors")
            .RequireAuthorization("ApiOrCookie");

        group.MapGet("/", ListAsync).WithName("ListVendors").WithSummary("List Purchasing.Vendor rows.");

        group.MapIntIdCrud<Vendor, VendorDto, CreateVendorRequest, UpdateVendorRequest, VendorAuditLog, VendorAuditLogDto, VendorAuditService.Snapshot, int>(
            entityName: "Vendor",
            routePrefix: "/api/aw/vendors",
            entitySet: db => db.Vendors,
            auditSet: db => db.VendorAuditLogs,
            idSelector: e => e.Id,
            auditIdSelector: a => a.VendorId,
            auditChangedDateSelector: a => a.ChangedDate,
            auditPrimaryKeySelector: a => a.Id,
            getId: e => e.Id,
            toDto: e => e.ToDto(),
            toEntity: r => r.ToEntity(),
            applyUpdate: (r, e) => r.ApplyTo(e),
            captureSnapshot: VendorAuditService.CaptureSnapshot,
            recordCreate: VendorAuditService.RecordCreate,
            recordUpdate: VendorAuditService.RecordUpdate,
            recordDelete: VendorAuditService.RecordDelete,
            auditToDto: a => a.ToDto());

        return app;
    }

    private static async Task<Ok<PagedResult<VendorDto>>> ListAsync(
        ApplicationDbContext db, [FromQuery] int skip = 0, [FromQuery] int take = 50,
        [FromQuery] string? name = null, [FromQuery] string? accountNumber = null, CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 1000);
        var query = db.Vendors.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(name)) query = query.Where(x => x.Name.Contains(name));
        if (!string.IsNullOrWhiteSpace(accountNumber)) query = query.Where(x => x.AccountNumber.Contains(accountNumber));
        var total = await query.CountAsync(ct);
        var rows = await query.OrderBy(x => x.Id).Skip(skip).Take(take).Select(x => x.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<VendorDto>(rows, total, skip, take));
    }
}