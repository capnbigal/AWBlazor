using System.Security.Claims;
using AWBlazorApp.Data;
using AWBlazorApp.Infrastructure.Persistence;
using AWBlazorApp.Models;
using AWBlazorApp.Features.AdventureWorks.Models;
using AWBlazorApp.Features.AdventureWorks.Audit;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.AdventureWorks.Endpoints;

public static class BusinessEntityContactEndpoints
{
    public static IEndpointRouteBuilder MapBusinessEntityContactEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/aw/business-entity-contacts")
            .WithTags("BusinessEntityContacts")
            .RequireAuthorization("ApiOrCookie");

        group.MapGet("/", ListAsync).WithName("ListBusinessEntityContacts")
            .WithSummary("List Person.BusinessEntityContact rows. 3-column composite PK.");
        group.MapGet("/by-key", GetAsync).WithName("GetBusinessEntityContact");
        group.MapPost("/", CreateAsync).WithName("CreateBusinessEntityContact")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));
        group.MapPatch("/by-key", UpdateAsync).WithName("UpdateBusinessEntityContact")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));
        group.MapDelete("/by-key", DeleteAsync).WithName("DeleteBusinessEntityContact")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));
        group.MapGet("/history", HistoryAsync).WithName("ListBusinessEntityContactHistory");
        return app;
    }

    private static async Task<Ok<PagedResult<BusinessEntityContactDto>>> ListAsync(
        ApplicationDbContext db, [FromQuery] int skip = 0, [FromQuery] int take = 50,
        [FromQuery] int? businessEntityId = null, CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 1000);
        var query = db.BusinessEntityContacts.AsNoTracking();
        if (businessEntityId.HasValue) query = query.Where(x => x.BusinessEntityId == businessEntityId.Value);
        var total = await query.CountAsync(ct);
        var rows = await query
            .OrderBy(x => x.BusinessEntityId).ThenBy(x => x.ContactTypeId).ThenBy(x => x.PersonId)
            .Skip(skip).Take(take).Select(x => x.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<BusinessEntityContactDto>(rows, total, skip, take));
    }

    private static async Task<Results<Ok<BusinessEntityContactDto>, NotFound>> GetAsync(
        [FromQuery] int businessEntityId, [FromQuery] int personId, [FromQuery] int contactTypeId,
        ApplicationDbContext db, CancellationToken ct)
    {
        var row = await db.BusinessEntityContacts.AsNoTracking()
            .FirstOrDefaultAsync(x =>
                x.BusinessEntityId == businessEntityId
                && x.PersonId == personId
                && x.ContactTypeId == contactTypeId, ct);
        return row is null ? TypedResults.NotFound() : TypedResults.Ok(row.ToDto());
    }

    private static async Task<Results<Created<CompositeKeyResponse>, Conflict<string>, ValidationProblem>> CreateAsync(
        CreateBusinessEntityContactRequest request, IValidator<CreateBusinessEntityContactRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());

        if (await db.BusinessEntityContacts.AnyAsync(x =>
                x.BusinessEntityId == request.BusinessEntityId
                && x.PersonId == request.PersonId
                && x.ContactTypeId == request.ContactTypeId, ct))
        {
            return TypedResults.Conflict(
                $"Junction row ({request.BusinessEntityId}, {request.PersonId}, {request.ContactTypeId}) already exists.");
        }

        var entity = request.ToEntity();
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        db.BusinessEntityContacts.Add(entity);
        await db.SaveChangesAsync(ct);
        db.BusinessEntityContactAuditLogs.Add(
            BusinessEntityContactAuditService.RecordCreate(entity, user.Identity?.Name));
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return TypedResults.Created(
            $"/api/aw/business-entity-contacts/by-key?businessEntityId={entity.BusinessEntityId}&personId={entity.PersonId}&contactTypeId={entity.ContactTypeId}",
            new CompositeKeyResponse(new Dictionary<string, object>
            {
                ["businessEntityId"] = entity.BusinessEntityId,
                ["personId"] = entity.PersonId,
                ["contactTypeId"] = entity.ContactTypeId,
            }));
    }

    private static async Task<Results<Ok<CompositeKeyResponse>, NotFound, ValidationProblem>> UpdateAsync(
        [FromQuery] int businessEntityId, [FromQuery] int personId, [FromQuery] int contactTypeId,
        UpdateBusinessEntityContactRequest request, IValidator<UpdateBusinessEntityContactRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());

        var entity = await db.BusinessEntityContacts
            .FirstOrDefaultAsync(x =>
                x.BusinessEntityId == businessEntityId
                && x.PersonId == personId
                && x.ContactTypeId == contactTypeId, ct);
        if (entity is null) return TypedResults.NotFound();

        request.ApplyTo(entity);
        db.BusinessEntityContactAuditLogs.Add(
            BusinessEntityContactAuditService.RecordUpdate(entity, user.Identity?.Name));
        await db.SaveChangesAsync(ct);
        return TypedResults.Ok(new CompositeKeyResponse(new Dictionary<string, object>
        {
            ["businessEntityId"] = entity.BusinessEntityId,
            ["personId"] = entity.PersonId,
            ["contactTypeId"] = entity.ContactTypeId,
        }));
    }

    private static async Task<Results<NoContent, NotFound>> DeleteAsync(
        [FromQuery] int businessEntityId, [FromQuery] int personId, [FromQuery] int contactTypeId,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var entity = await db.BusinessEntityContacts
            .FirstOrDefaultAsync(x =>
                x.BusinessEntityId == businessEntityId
                && x.PersonId == personId
                && x.ContactTypeId == contactTypeId, ct);
        if (entity is null) return TypedResults.NotFound();

        db.BusinessEntityContactAuditLogs.Add(
            BusinessEntityContactAuditService.RecordDelete(entity, user.Identity?.Name));
        db.BusinessEntityContacts.Remove(entity);
        await db.SaveChangesAsync(ct);
        return TypedResults.NoContent();
    }

    private static async Task<Ok<List<BusinessEntityContactAuditLogDto>>> HistoryAsync(
        ApplicationDbContext db,
        [FromQuery] int? businessEntityId = null,
        [FromQuery] int? personId = null,
        [FromQuery] int? contactTypeId = null,
        CancellationToken ct = default)
    {
        var query = db.BusinessEntityContactAuditLogs.AsNoTracking();
        if (businessEntityId.HasValue) query = query.Where(a => a.BusinessEntityId == businessEntityId.Value);
        if (personId.HasValue) query = query.Where(a => a.PersonId == personId.Value);
        if (contactTypeId.HasValue) query = query.Where(a => a.ContactTypeId == contactTypeId.Value);

        var rows = await query
            .OrderByDescending(a => a.ChangedDate).ThenByDescending(a => a.Id)
            .Select(a => a.ToDto())
            .ToListAsync(ct);
        return TypedResults.Ok(rows);
    }
}
