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

namespace AWBlazorApp.Features.Person.Persons.Api;

public static class PersonEndpoints
{
    public static IEndpointRouteBuilder MapPersonEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/aw/persons")
            .WithTags("Persons")
            .RequireAuthorization("ApiOrCookie");

        group.MapGet("/", ListAsync).WithName("ListPersons").WithSummary("List Person.Person rows.");

        group.MapIntIdCrud<AWBlazorApp.Features.Person.Persons.Domain.Person, PersonDto, CreatePersonRequest, UpdatePersonRequest, PersonAuditLog, PersonAuditLogDto, PersonAuditService.Snapshot, int>(
            entityName: "Person",
            routePrefix: "/api/aw/persons",
            entitySet: db => db.Persons,
            auditSet: db => db.PersonAuditLogs,
            idSelector: e => e.Id,
            auditIdSelector: a => a.PersonId,
            auditChangedDateSelector: a => a.ChangedDate,
            auditPrimaryKeySelector: a => a.Id,
            getId: e => e.Id,
            toDto: e => e.ToDto(),
            toEntity: r => r.ToEntity(),
            applyUpdate: (r, e) => r.ApplyTo(e),
            captureSnapshot: PersonAuditService.CaptureSnapshot,
            recordCreate: PersonAuditService.RecordCreate,
            recordUpdate: PersonAuditService.RecordUpdate,
            recordDelete: PersonAuditService.RecordDelete,
            auditToDto: a => a.ToDto());

        return app;
    }

    private static async Task<Ok<PagedResult<PersonDto>>> ListAsync(
        ApplicationDbContext db, [FromQuery] int skip = 0, [FromQuery] int take = 50,
        [FromQuery] string? lastName = null, [FromQuery] string? personType = null,
        CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 1000);
        var query = db.Persons.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(lastName)) query = query.Where(x => x.LastName.Contains(lastName));
        if (!string.IsNullOrWhiteSpace(personType)) query = query.Where(x => x.PersonType == personType);
        var total = await query.CountAsync(ct);
        var rows = await query.OrderBy(x => x.LastName).ThenBy(x => x.FirstName).ThenBy(x => x.Id)
            .Skip(skip).Take(take).Select(x => x.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<PersonDto>(rows, total, skip, take));
    }
}