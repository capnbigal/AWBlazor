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

public static class PhoneNumberTypeEndpoints
{
    public static IEndpointRouteBuilder MapPhoneNumberTypeEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/aw/phone-number-types")
            .WithTags("PhoneNumberTypes")
            .RequireAuthorization("ApiOrCookie");

        group.MapGet("/", ListAsync).WithName("ListPhoneNumberTypes").WithSummary("List Person.PhoneNumberType rows.");

        group.MapIntIdCrud<PhoneNumberType, PhoneNumberTypeDto, CreatePhoneNumberTypeRequest, UpdatePhoneNumberTypeRequest, PhoneNumberTypeAuditLog, PhoneNumberTypeAuditLogDto, PhoneNumberTypeAuditService.Snapshot, int>(
            entityName: "PhoneNumberType",
            routePrefix: "/api/aw/phone-number-types",
            entitySet: db => db.PhoneNumberTypes,
            auditSet: db => db.PhoneNumberTypeAuditLogs,
            idSelector: e => e.Id,
            auditIdSelector: a => a.PhoneNumberTypeId,
            auditChangedDateSelector: a => a.ChangedDate,
            auditPrimaryKeySelector: a => a.Id,
            getId: e => e.Id,
            toDto: e => e.ToDto(),
            toEntity: r => r.ToEntity(),
            applyUpdate: (r, e) => r.ApplyTo(e),
            captureSnapshot: PhoneNumberTypeAuditService.CaptureSnapshot,
            recordCreate: PhoneNumberTypeAuditService.RecordCreate,
            recordUpdate: PhoneNumberTypeAuditService.RecordUpdate,
            recordDelete: PhoneNumberTypeAuditService.RecordDelete,
            auditToDto: a => a.ToDto());

        return app;
    }

    private static async Task<Ok<PagedResult<PhoneNumberTypeDto>>> ListAsync(
        ApplicationDbContext db, [FromQuery] int skip = 0, [FromQuery] int take = 50,
        [FromQuery] string? name = null, CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 1000);
        var query = db.PhoneNumberTypes.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(name)) query = query.Where(x => x.Name.Contains(name));
        var total = await query.CountAsync(ct);
        var rows = await query.OrderBy(x => x.Id).Skip(skip).Take(take).Select(x => x.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<PhoneNumberTypeDto>(rows, total, skip, take));
    }
}