using System.Security.Claims;
using AWBlazorApp.Data;
using AWBlazorApp.Infrastructure.Persistence;
using AWBlazorApp.Shared.Models;
using AWBlazorApp.Features.AdventureWorks.Models;
using AWBlazorApp.Features.AdventureWorks.Audit;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.AdventureWorks.Endpoints;

public static class EmailAddressEndpoints
{
    public static IEndpointRouteBuilder MapEmailAddressEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/aw/email-addresses")
            .WithTags("EmailAddresses")
            .RequireAuthorization("ApiOrCookie");

        group.MapGet("/", ListAsync).WithName("ListEmailAddresses")
            .WithSummary("List Person.EmailAddress rows. Composite PK = (BusinessEntityID, EmailAddressID).");
        group.MapGet("/by-key", GetAsync).WithName("GetEmailAddress");
        group.MapPost("/", CreateAsync).WithName("CreateEmailAddress")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));
        group.MapPatch("/by-key", UpdateAsync).WithName("UpdateEmailAddress")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));
        group.MapDelete("/by-key", DeleteAsync).WithName("DeleteEmailAddress")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));
        group.MapGet("/history", HistoryAsync).WithName("ListEmailAddressHistory");
        return app;
    }

    private static async Task<Ok<PagedResult<EmailAddressDto>>> ListAsync(
        ApplicationDbContext db, [FromQuery] int skip = 0, [FromQuery] int take = 50,
        [FromQuery] int? businessEntityId = null, [FromQuery] string? emailContains = null,
        CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 1000);
        var query = db.EmailAddresses.AsNoTracking();
        if (businessEntityId.HasValue) query = query.Where(x => x.BusinessEntityId == businessEntityId.Value);
        if (!string.IsNullOrWhiteSpace(emailContains))
            query = query.Where(x => x.EmailAddressValue != null && x.EmailAddressValue.Contains(emailContains));
        var total = await query.CountAsync(ct);
        var rows = await query.OrderBy(x => x.BusinessEntityId).ThenBy(x => x.EmailAddressId)
            .Skip(skip).Take(take).Select(x => x.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<EmailAddressDto>(rows, total, skip, take));
    }

    private static async Task<Results<Ok<EmailAddressDto>, NotFound>> GetAsync(
        [FromQuery] int businessEntityId, [FromQuery] int emailAddressId,
        ApplicationDbContext db, CancellationToken ct)
    {
        var row = await db.EmailAddresses.AsNoTracking()
            .FirstOrDefaultAsync(x => x.BusinessEntityId == businessEntityId && x.EmailAddressId == emailAddressId, ct);
        return row is null ? TypedResults.NotFound() : TypedResults.Ok(row.ToDto());
    }

    private static async Task<Results<Created<CompositeKeyResponse>, ValidationProblem>> CreateAsync(
        CreateEmailAddressRequest request, IValidator<CreateEmailAddressRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());

        var entity = request.ToEntity();
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        db.EmailAddresses.Add(entity);
        await db.SaveChangesAsync(ct);
        db.EmailAddressAuditLogs.Add(EmailAddressAuditService.RecordCreate(entity, user.Identity?.Name));
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return TypedResults.Created(
            $"/api/aw/email-addresses/by-key?businessEntityId={entity.BusinessEntityId}&emailAddressId={entity.EmailAddressId}",
            new CompositeKeyResponse(new Dictionary<string, object>
            {
                ["businessEntityId"] = entity.BusinessEntityId,
                ["emailAddressId"] = entity.EmailAddressId,
            }));
    }

    private static async Task<Results<Ok<CompositeKeyResponse>, NotFound, ValidationProblem>> UpdateAsync(
        [FromQuery] int businessEntityId, [FromQuery] int emailAddressId,
        UpdateEmailAddressRequest request, IValidator<UpdateEmailAddressRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());

        var entity = await db.EmailAddresses
            .FirstOrDefaultAsync(x => x.BusinessEntityId == businessEntityId && x.EmailAddressId == emailAddressId, ct);
        if (entity is null) return TypedResults.NotFound();

        var before = EmailAddressAuditService.CaptureSnapshot(entity);
        request.ApplyTo(entity);
        db.EmailAddressAuditLogs.Add(EmailAddressAuditService.RecordUpdate(before, entity, user.Identity?.Name));
        await db.SaveChangesAsync(ct);
        return TypedResults.Ok(new CompositeKeyResponse(new Dictionary<string, object>
        {
            ["businessEntityId"] = entity.BusinessEntityId,
            ["emailAddressId"] = entity.EmailAddressId,
        }));
    }

    private static async Task<Results<NoContent, NotFound>> DeleteAsync(
        [FromQuery] int businessEntityId, [FromQuery] int emailAddressId,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var entity = await db.EmailAddresses
            .FirstOrDefaultAsync(x => x.BusinessEntityId == businessEntityId && x.EmailAddressId == emailAddressId, ct);
        if (entity is null) return TypedResults.NotFound();

        db.EmailAddressAuditLogs.Add(EmailAddressAuditService.RecordDelete(entity, user.Identity?.Name));
        db.EmailAddresses.Remove(entity);
        await db.SaveChangesAsync(ct);
        return TypedResults.NoContent();
    }

    private static async Task<Ok<List<EmailAddressAuditLogDto>>> HistoryAsync(
        ApplicationDbContext db,
        [FromQuery] int? businessEntityId = null,
        [FromQuery] int? emailAddressId = null,
        CancellationToken ct = default)
    {
        var query = db.EmailAddressAuditLogs.AsNoTracking();
        if (businessEntityId.HasValue) query = query.Where(a => a.BusinessEntityId == businessEntityId.Value);
        if (emailAddressId.HasValue) query = query.Where(a => a.EmailAddressId == emailAddressId.Value);

        var rows = await query
            .OrderByDescending(a => a.ChangedDate).ThenByDescending(a => a.Id)
            .Select(a => a.ToDto())
            .ToListAsync(ct);
        return TypedResults.Ok(rows);
    }
}
