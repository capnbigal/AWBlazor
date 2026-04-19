using AWBlazorApp.Features.Identity.Domain; using AWBlazorApp.Features.Admin.Permissions.Domain;
using AWBlazorApp.Infrastructure.Persistence;
using AWBlazorApp.Shared.Api;
using AWBlazorApp.Shared.Dtos;
using AWBlazorApp.Features.Sales.Audit;
using AWBlazorApp.Features.Sales.Domain;
using AWBlazorApp.Features.Sales.Dtos;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.Sales.Api;

public static class CustomerEndpoints
{
    public static IEndpointRouteBuilder MapCustomerEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/aw/customers")
            .WithTags("Customers")
            .RequireAuthorization("ApiOrCookie");

        group.MapGet("/", ListAsync).WithName("ListCustomers").WithSummary("List Sales.Customer rows.");

        group.MapIntIdCrud<Customer, CustomerDto, CreateCustomerRequest, UpdateCustomerRequest, CustomerAuditLog, CustomerAuditLogDto, CustomerAuditService.Snapshot, int>(
            entityName: "Customer",
            routePrefix: "/api/aw/customers",
            entitySet: db => db.Customers,
            auditSet: db => db.CustomerAuditLogs,
            idSelector: e => e.Id,
            auditIdSelector: a => a.CustomerId,
            auditChangedDateSelector: a => a.ChangedDate,
            auditPrimaryKeySelector: a => a.Id,
            getId: e => e.Id,
            toDto: e => e.ToDto(),
            toEntity: r => r.ToEntity(),
            applyUpdate: (r, e) => r.ApplyTo(e),
            captureSnapshot: CustomerAuditService.CaptureSnapshot,
            recordCreate: CustomerAuditService.RecordCreate,
            recordUpdate: CustomerAuditService.RecordUpdate,
            recordDelete: CustomerAuditService.RecordDelete,
            auditToDto: a => a.ToDto());

        return app;
    }

    private static async Task<Ok<PagedResult<CustomerDto>>> ListAsync(
        ApplicationDbContext db, [FromQuery] int skip = 0, [FromQuery] int take = 50,
        [FromQuery] int? territoryId = null, CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 1000);
        var query = db.Customers.AsNoTracking();
        if (territoryId.HasValue) query = query.Where(x => x.TerritoryId == territoryId.Value);
        var total = await query.CountAsync(ct);
        var rows = await query.OrderBy(x => x.Id).Skip(skip).Take(take).Select(x => x.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<CustomerDto>(rows, total, skip, take));
    }
}