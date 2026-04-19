using System.Security.Claims;
using AWBlazorApp.Data;
using AWBlazorApp.Infrastructure.Persistence;
using AWBlazorApp.Shared.Models;
using AWBlazorApp.Features.Person.Models;
using AWBlazorApp.Features.Person.Audit;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.Person.Endpoints;

public static class BusinessEntityAddressEndpoints
{
    public static IEndpointRouteBuilder MapBusinessEntityAddressEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/aw/business-entity-addresses")
            .WithTags("BusinessEntityAddresses")
            .RequireAuthorization("ApiOrCookie");

        group.MapGet("/", ListAsync).WithName("ListBusinessEntityAddresses")
            .WithSummary("List Person.BusinessEntityAddress rows. 3-column composite PK.");
        group.MapGet("/by-key", GetAsync).WithName("GetBusinessEntityAddress");
        group.MapPost("/", CreateAsync).WithName("CreateBusinessEntityAddress")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));
        group.MapPatch("/by-key", UpdateAsync).WithName("UpdateBusinessEntityAddress")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));
        group.MapDelete("/by-key", DeleteAsync).WithName("DeleteBusinessEntityAddress")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));
        group.MapGet("/history", HistoryAsync).WithName("ListBusinessEntityAddressHistory");
        return app;
    }

    private static async Task<Ok<PagedResult<BusinessEntityAddressDto>>> ListAsync(
        ApplicationDbContext db, [FromQuery] int skip = 0, [FromQuery] int take = 50,
        [FromQuery] int? businessEntityId = null, CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 1000);
        var query = db.BusinessEntityAddresses.AsNoTracking();
        if (businessEntityId.HasValue) query = query.Where(x => x.BusinessEntityId == businessEntityId.Value);
        var total = await query.CountAsync(ct);
        var rows = await query
            .OrderBy(x => x.BusinessEntityId).ThenBy(x => x.AddressTypeId).ThenBy(x => x.AddressId)
            .Skip(skip).Take(take).Select(x => x.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<BusinessEntityAddressDto>(rows, total, skip, take));
    }

    private static async Task<Results<Ok<BusinessEntityAddressDto>, NotFound>> GetAsync(
        [FromQuery] int businessEntityId, [FromQuery] int addressId, [FromQuery] int addressTypeId,
        ApplicationDbContext db, CancellationToken ct)
    {
        var row = await db.BusinessEntityAddresses.AsNoTracking()
            .FirstOrDefaultAsync(x =>
                x.BusinessEntityId == businessEntityId
                && x.AddressId == addressId
                && x.AddressTypeId == addressTypeId, ct);
        return row is null ? TypedResults.NotFound() : TypedResults.Ok(row.ToDto());
    }

    private static async Task<Results<Created<CompositeKeyResponse>, Conflict<string>, ValidationProblem>> CreateAsync(
        CreateBusinessEntityAddressRequest request, IValidator<CreateBusinessEntityAddressRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());

        if (await db.BusinessEntityAddresses.AnyAsync(x =>
                x.BusinessEntityId == request.BusinessEntityId
                && x.AddressId == request.AddressId
                && x.AddressTypeId == request.AddressTypeId, ct))
        {
            return TypedResults.Conflict(
                $"Junction row ({request.BusinessEntityId}, {request.AddressId}, {request.AddressTypeId}) already exists.");
        }

        var entity = request.ToEntity();
        await db.AddWithAuditAsync(entity, e => BusinessEntityAddressAuditService.RecordCreate(e, user.Identity?.Name), ct);
        return TypedResults.Created(
            $"/api/aw/business-entity-addresses/by-key?businessEntityId={entity.BusinessEntityId}&addressId={entity.AddressId}&addressTypeId={entity.AddressTypeId}",
            new CompositeKeyResponse(new Dictionary<string, object>
            {
                ["businessEntityId"] = entity.BusinessEntityId,
                ["addressId"] = entity.AddressId,
                ["addressTypeId"] = entity.AddressTypeId,
            }));
    }

    private static async Task<Results<Ok<CompositeKeyResponse>, NotFound, ValidationProblem>> UpdateAsync(
        [FromQuery] int businessEntityId, [FromQuery] int addressId, [FromQuery] int addressTypeId,
        UpdateBusinessEntityAddressRequest request, IValidator<UpdateBusinessEntityAddressRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());

        var entity = await db.BusinessEntityAddresses
            .FirstOrDefaultAsync(x =>
                x.BusinessEntityId == businessEntityId
                && x.AddressId == addressId
                && x.AddressTypeId == addressTypeId, ct);
        if (entity is null) return TypedResults.NotFound();

        request.ApplyTo(entity);
        db.BusinessEntityAddressAuditLogs.Add(
            BusinessEntityAddressAuditService.RecordUpdate(entity, user.Identity?.Name));
        await db.SaveChangesAsync(ct);
        return TypedResults.Ok(new CompositeKeyResponse(new Dictionary<string, object>
        {
            ["businessEntityId"] = entity.BusinessEntityId,
            ["addressId"] = entity.AddressId,
            ["addressTypeId"] = entity.AddressTypeId,
        }));
    }

    private static async Task<Results<NoContent, NotFound>> DeleteAsync(
        [FromQuery] int businessEntityId, [FromQuery] int addressId, [FromQuery] int addressTypeId,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var entity = await db.BusinessEntityAddresses
            .FirstOrDefaultAsync(x =>
                x.BusinessEntityId == businessEntityId
                && x.AddressId == addressId
                && x.AddressTypeId == addressTypeId, ct);
        if (entity is null) return TypedResults.NotFound();

        db.BusinessEntityAddressAuditLogs.Add(
            BusinessEntityAddressAuditService.RecordDelete(entity, user.Identity?.Name));
        db.BusinessEntityAddresses.Remove(entity);
        await db.SaveChangesAsync(ct);
        return TypedResults.NoContent();
    }

    private static async Task<Ok<List<BusinessEntityAddressAuditLogDto>>> HistoryAsync(
        ApplicationDbContext db,
        [FromQuery] int? businessEntityId = null,
        [FromQuery] int? addressId = null,
        [FromQuery] int? addressTypeId = null,
        CancellationToken ct = default)
    {
        var query = db.BusinessEntityAddressAuditLogs.AsNoTracking();
        if (businessEntityId.HasValue) query = query.Where(a => a.BusinessEntityId == businessEntityId.Value);
        if (addressId.HasValue) query = query.Where(a => a.AddressId == addressId.Value);
        if (addressTypeId.HasValue) query = query.Where(a => a.AddressTypeId == addressTypeId.Value);

        var rows = await query
            .OrderByDescending(a => a.ChangedDate).ThenByDescending(a => a.Id)
            .Select(a => a.ToDto())
            .ToListAsync(ct);
        return TypedResults.Ok(rows);
    }
}
