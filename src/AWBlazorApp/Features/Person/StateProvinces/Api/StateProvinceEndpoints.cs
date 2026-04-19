using AWBlazorApp.Features.Identity.Domain; using AWBlazorApp.Features.Admin.Permissions.Domain;
using AWBlazorApp.Infrastructure.Persistence;
using AWBlazorApp.Shared.Api;
using AWBlazorApp.Shared.Dtos;
using AWBlazorApp.Features.Person.Addresses.Application.Services; using AWBlazorApp.Features.Person.AddressTypes.Application.Services; using AWBlazorApp.Features.Person.BusinessEntities.Application.Services; using AWBlazorApp.Features.Person.BusinessEntityAddresses.Application.Services; using AWBlazorApp.Features.Person.BusinessEntityContacts.Application.Services; using AWBlazorApp.Features.Person.ContactTypes.Application.Services; using AWBlazorApp.Features.Person.CountryRegions.Application.Services; using AWBlazorApp.Features.Person.EmailAddresses.Application.Services; using AWBlazorApp.Features.Person.Persons.Application.Services; using AWBlazorApp.Features.Person.PersonPhones.Application.Services; using AWBlazorApp.Features.Person.PhoneNumberTypes.Application.Services; using AWBlazorApp.Features.Person.StateProvinces.Application.Services; 
using AWBlazorApp.Features.Person.Addresses.Domain; using AWBlazorApp.Features.Person.AddressTypes.Domain; using AWBlazorApp.Features.Person.BusinessEntities.Domain; using AWBlazorApp.Features.Person.BusinessEntityAddresses.Domain; using AWBlazorApp.Features.Person.BusinessEntityContacts.Domain; using AWBlazorApp.Features.Person.ContactTypes.Domain; using AWBlazorApp.Features.Person.CountryRegions.Domain; using AWBlazorApp.Features.Person.EmailAddresses.Domain; using AWBlazorApp.Features.Person.Persons.Domain; using AWBlazorApp.Features.Person.PersonPhones.Domain; using AWBlazorApp.Features.Person.PhoneNumberTypes.Domain; using AWBlazorApp.Features.Person.StateProvinces.Domain; 
using AWBlazorApp.Features.Person.Addresses.Dtos; using AWBlazorApp.Features.Person.AddressTypes.Dtos; using AWBlazorApp.Features.Person.BusinessEntities.Dtos; using AWBlazorApp.Features.Person.BusinessEntityAddresses.Dtos; using AWBlazorApp.Features.Person.BusinessEntityContacts.Dtos; using AWBlazorApp.Features.Person.ContactTypes.Dtos; using AWBlazorApp.Features.Person.CountryRegions.Dtos; using AWBlazorApp.Features.Person.EmailAddresses.Dtos; using AWBlazorApp.Features.Person.Persons.Dtos; using AWBlazorApp.Features.Person.PersonPhones.Dtos; using AWBlazorApp.Features.Person.PhoneNumberTypes.Dtos; using AWBlazorApp.Features.Person.StateProvinces.Dtos; 
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.Person.StateProvinces.Api;

public static class StateProvinceEndpoints
{
    public static IEndpointRouteBuilder MapStateProvinceEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/aw/state-provinces")
            .WithTags("StateProvinces")
            .RequireAuthorization("ApiOrCookie");

        group.MapGet("/", ListAsync).WithName("ListStateProvinces").WithSummary("List Person.StateProvince rows.");

        group.MapIntIdCrud<StateProvince, StateProvinceDto, CreateStateProvinceRequest, UpdateStateProvinceRequest, StateProvinceAuditLog, StateProvinceAuditLogDto, StateProvinceAuditService.Snapshot, int>(
            entityName: "StateProvince",
            routePrefix: "/api/aw/state-provinces",
            entitySet: db => db.StateProvinces,
            auditSet: db => db.StateProvinceAuditLogs,
            idSelector: e => e.Id,
            auditIdSelector: a => a.StateProvinceId,
            auditChangedDateSelector: a => a.ChangedDate,
            auditPrimaryKeySelector: a => a.Id,
            getId: e => e.Id,
            toDto: e => e.ToDto(),
            toEntity: r => r.ToEntity(),
            applyUpdate: (r, e) => r.ApplyTo(e),
            captureSnapshot: StateProvinceAuditService.CaptureSnapshot,
            recordCreate: StateProvinceAuditService.RecordCreate,
            recordUpdate: StateProvinceAuditService.RecordUpdate,
            recordDelete: StateProvinceAuditService.RecordDelete,
            auditToDto: a => a.ToDto());

        return app;
    }

    private static async Task<Ok<PagedResult<StateProvinceDto>>> ListAsync(
        ApplicationDbContext db, [FromQuery] int skip = 0, [FromQuery] int take = 50,
        [FromQuery] string? name = null, [FromQuery] string? countryRegionCode = null, CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 1000);
        var query = db.StateProvinces.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(name)) query = query.Where(x => x.Name.Contains(name));
        if (!string.IsNullOrWhiteSpace(countryRegionCode)) query = query.Where(x => x.CountryRegionCode == countryRegionCode);
        var total = await query.CountAsync(ct);
        var rows = await query.OrderBy(x => x.Id).Skip(skip).Take(take).Select(x => x.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<StateProvinceDto>(rows, total, skip, take));
    }
}