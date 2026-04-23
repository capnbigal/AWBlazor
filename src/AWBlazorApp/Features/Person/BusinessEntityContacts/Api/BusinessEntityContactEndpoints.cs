using System.Security.Claims;
using AWBlazorApp.Features.Identity.Domain; using AWBlazorApp.Features.Admin.Permissions.Domain;
using AWBlazorApp.Infrastructure.Persistence;
using AWBlazorApp.Shared.Dtos;
using AWBlazorApp.Features.Person.Addresses.Dtos; using AWBlazorApp.Features.Person.AddressTypes.Dtos; using AWBlazorApp.Features.Person.BusinessEntities.Dtos; using AWBlazorApp.Features.Person.BusinessEntityAddresses.Dtos; using AWBlazorApp.Features.Person.BusinessEntityContacts.Dtos; using AWBlazorApp.Features.Person.ContactTypes.Dtos; using AWBlazorApp.Features.Person.CountryRegions.Dtos; using AWBlazorApp.Features.Person.EmailAddresses.Dtos; using AWBlazorApp.Features.Person.Persons.Dtos; using AWBlazorApp.Features.Person.PersonPhones.Dtos; using AWBlazorApp.Features.Person.PhoneNumberTypes.Dtos; using AWBlazorApp.Features.Person.StateProvinces.Dtos; 
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.Person.BusinessEntityContacts.Api;

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

        db.BusinessEntityContacts.Remove(entity);
        await db.SaveChangesAsync(ct);
        return TypedResults.NoContent();
    }

    private static async Task<Ok<List<AWBlazorApp.Shared.Audit.AuditLog>>> HistoryAsync(
        ApplicationDbContext db,
        CancellationToken ct = default)
    {
        var rows = await db.AuditLogs.AsNoTracking()
            .Where(a => a.EntityType == "BusinessEntityContact")
            .OrderByDescending(a => a.ChangedDate).ThenByDescending(a => a.Id)
            .ToListAsync(ct);
        return TypedResults.Ok(rows);
    }

}
