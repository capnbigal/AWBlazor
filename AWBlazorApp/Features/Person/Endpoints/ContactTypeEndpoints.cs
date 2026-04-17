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

public static class ContactTypeEndpoints
{
    public static IEndpointRouteBuilder MapContactTypeEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/aw/contact-types")
            .WithTags("ContactTypes")
            .RequireAuthorization("ApiOrCookie");

        group.MapGet("/", ListAsync).WithName("ListContactTypes").WithSummary("List Person.ContactType rows.");

        group.MapIntIdCrud<ContactType, ContactTypeDto, CreateContactTypeRequest, UpdateContactTypeRequest, ContactTypeAuditLog, ContactTypeAuditLogDto, ContactTypeAuditService.Snapshot, int>(
            entityName: "ContactType",
            routePrefix: "/api/aw/contact-types",
            entitySet: db => db.ContactTypes,
            auditSet: db => db.ContactTypeAuditLogs,
            idSelector: e => e.Id,
            auditIdSelector: a => a.ContactTypeId,
            auditChangedDateSelector: a => a.ChangedDate,
            auditPrimaryKeySelector: a => a.Id,
            getId: e => e.Id,
            toDto: e => e.ToDto(),
            toEntity: r => r.ToEntity(),
            applyUpdate: (r, e) => r.ApplyTo(e),
            captureSnapshot: ContactTypeAuditService.CaptureSnapshot,
            recordCreate: ContactTypeAuditService.RecordCreate,
            recordUpdate: ContactTypeAuditService.RecordUpdate,
            recordDelete: ContactTypeAuditService.RecordDelete,
            auditToDto: a => a.ToDto());

        return app;
    }

    private static async Task<Ok<PagedResult<ContactTypeDto>>> ListAsync(
        ApplicationDbContext db,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 50,
        [FromQuery] string? name = null,
        CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 1000);
        var query = db.ContactTypes.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(name)) query = query.Where(x => x.Name.Contains(name));
        var total = await query.CountAsync(ct);
        var rows = await query.OrderBy(x => x.Id).Skip(skip).Take(take).Select(x => x.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<ContactTypeDto>(rows, total, skip, take));
    }
}