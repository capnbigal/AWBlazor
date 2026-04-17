using AWBlazorApp.Data;
using AWBlazorApp.Infrastructure.Persistence;
using AWBlazorApp.Shared.Endpoints;
using AWBlazorApp.Shared.Models;
using AWBlazorApp.Features.Person.Audit;
using AWBlazorApp.Features.Person.Domain;
using AWBlazorApp.Features.Person.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.Person.Endpoints;

public static class AddressEndpoints
{
    public static IEndpointRouteBuilder MapAddressEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/aw/addresses")
            .WithTags("Addresses")
            .RequireAuthorization("ApiOrCookie");

        group.MapGet("/", ListAsync).WithName("ListAddresses").WithSummary("List Person.Address rows.");

        group.MapIntIdCrud<Address, AddressDto, CreateAddressRequest, UpdateAddressRequest, AddressAuditLog, AddressAuditLogDto, AddressAuditService.Snapshot, int>(
            entityName: "Address",
            routePrefix: "/api/aw/addresses",
            entitySet: db => db.Addresses,
            auditSet: db => db.AddressAuditLogs,
            idSelector: e => e.Id,
            auditIdSelector: a => a.AddressId,
            auditChangedDateSelector: a => a.ChangedDate,
            auditPrimaryKeySelector: a => a.Id,
            getId: e => e.Id,
            toDto: e => e.ToDto(),
            toEntity: r => r.ToEntity(),
            applyUpdate: (r, e) => r.ApplyTo(e),
            captureSnapshot: AddressAuditService.CaptureSnapshot,
            recordCreate: AddressAuditService.RecordCreate,
            recordUpdate: AddressAuditService.RecordUpdate,
            recordDelete: AddressAuditService.RecordDelete,
            auditToDto: a => a.ToDto());

        return app;
    }

    private static async Task<Ok<PagedResult<AddressDto>>> ListAsync(
        ApplicationDbContext db, [FromQuery] int skip = 0, [FromQuery] int take = 50,
        [FromQuery] string? city = null, [FromQuery] string? postalCode = null,
        CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 1000);
        var query = db.Addresses.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(city)) query = query.Where(x => x.City.Contains(city));
        if (!string.IsNullOrWhiteSpace(postalCode)) query = query.Where(x => x.PostalCode.StartsWith(postalCode));
        var total = await query.CountAsync(ct);
        var rows = await query.OrderBy(x => x.Id).Skip(skip).Take(take).Select(x => x.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<AddressDto>(rows, total, skip, take));
    }
}