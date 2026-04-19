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

public static class PersonEndpoints
{
    public static IEndpointRouteBuilder MapPersonEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/aw/persons")
            .WithTags("Persons")
            .RequireAuthorization("ApiOrCookie");

        group.MapGet("/", ListAsync).WithName("ListPersons").WithSummary("List Person.Person rows.");

        group.MapIntIdCrud<AWBlazorApp.Features.Person.Domain.Person, PersonDto, CreatePersonRequest, UpdatePersonRequest, PersonAuditLog, PersonAuditLogDto, PersonAuditService.Snapshot, int>(
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