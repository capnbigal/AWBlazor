using AWBlazorApp.Features.Identity.Domain; using AWBlazorApp.Features.Admin.Permissions.Domain;
using AWBlazorApp.Infrastructure.Persistence;
using AWBlazorApp.Shared.Api;
using AWBlazorApp.Shared.Dtos;
using AWBlazorApp.Features.Person.Audit;
using AWBlazorApp.Features.Person.Domain;
using AWBlazorApp.Features.Person.Dtos;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.Person.Api;

public static class AddressTypeEndpoints
{
    public static IEndpointRouteBuilder MapAddressTypeEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/aw/address-types")
            .WithTags("AddressTypes")
            .RequireAuthorization("ApiOrCookie");

        group.MapGet("/", ListAsync)
            .WithName("ListAddressTypes")
            .WithSummary("List AdventureWorks Person.AddressType rows.");

        group.MapIntIdCrud<AddressType, AddressTypeDto, CreateAddressTypeRequest, UpdateAddressTypeRequest, AddressTypeAuditLog, AddressTypeAuditLogDto, AddressTypeAuditService.Snapshot, int>(
            entityName: "AddressType",
            routePrefix: "/api/aw/address-types",
            entitySet: db => db.AddressTypes,
            auditSet: db => db.AddressTypeAuditLogs,
            idSelector: e => e.Id,
            auditIdSelector: a => a.AddressTypeId,
            auditChangedDateSelector: a => a.ChangedDate,
            auditPrimaryKeySelector: a => a.Id,
            getId: e => e.Id,
            toDto: e => e.ToDto(),
            toEntity: r => r.ToEntity(),
            applyUpdate: (r, e) => r.ApplyTo(e),
            captureSnapshot: AddressTypeAuditService.CaptureSnapshot,
            recordCreate: AddressTypeAuditService.RecordCreate,
            recordUpdate: AddressTypeAuditService.RecordUpdate,
            recordDelete: AddressTypeAuditService.RecordDelete,
            auditToDto: a => a.ToDto());

        return app;
    }

    private static async Task<Ok<PagedResult<AddressTypeDto>>> ListAsync(
        ApplicationDbContext db,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 50,
        [FromQuery] string? name = null,
        CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 1000);
        var query = db.AddressTypes.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(name)) query = query.Where(x => x.Name.Contains(name));

        var total = await query.CountAsync(ct);
        var rows = await query.OrderBy(x => x.Id)
            .Skip(skip).Take(take)
            .Select(x => x.ToDto())
            .ToListAsync(ct);

        return TypedResults.Ok(new PagedResult<AddressTypeDto>(rows, total, skip, take));
    }
}