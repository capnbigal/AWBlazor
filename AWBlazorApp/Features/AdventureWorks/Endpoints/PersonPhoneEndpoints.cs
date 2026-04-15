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

public static class PersonPhoneEndpoints
{
    public static IEndpointRouteBuilder MapPersonPhoneEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/aw/person-phones")
            .WithTags("PersonPhones")
            .RequireAuthorization("ApiOrCookie");

        group.MapGet("/", ListAsync).WithName("ListPersonPhones")
            .WithSummary("List Person.PersonPhone rows. 3-column composite PK = (BusinessEntityID, PhoneNumber, PhoneNumberTypeID).");
        group.MapGet("/by-key", GetAsync).WithName("GetPersonPhone");
        group.MapPost("/", CreateAsync).WithName("CreatePersonPhone")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));
        group.MapPatch("/by-key", UpdateAsync).WithName("UpdatePersonPhone")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));
        group.MapDelete("/by-key", DeleteAsync).WithName("DeletePersonPhone")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));
        group.MapGet("/history", HistoryAsync).WithName("ListPersonPhoneHistory");
        return app;
    }

    private static async Task<Ok<PagedResult<PersonPhoneDto>>> ListAsync(
        ApplicationDbContext db, [FromQuery] int skip = 0, [FromQuery] int take = 50,
        [FromQuery] int? businessEntityId = null, [FromQuery] int? phoneNumberTypeId = null,
        CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 1000);
        var query = db.PersonPhones.AsNoTracking();
        if (businessEntityId.HasValue) query = query.Where(x => x.BusinessEntityId == businessEntityId.Value);
        if (phoneNumberTypeId.HasValue) query = query.Where(x => x.PhoneNumberTypeId == phoneNumberTypeId.Value);
        var total = await query.CountAsync(ct);
        var rows = await query
            .OrderBy(x => x.BusinessEntityId).ThenBy(x => x.PhoneNumberTypeId).ThenBy(x => x.PhoneNumber)
            .Skip(skip).Take(take).Select(x => x.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<PersonPhoneDto>(rows, total, skip, take));
    }

    private static async Task<Results<Ok<PersonPhoneDto>, NotFound>> GetAsync(
        [FromQuery] int businessEntityId, [FromQuery] string phoneNumber, [FromQuery] int phoneNumberTypeId,
        ApplicationDbContext db, CancellationToken ct)
    {
        var row = await db.PersonPhones.AsNoTracking()
            .FirstOrDefaultAsync(x =>
                x.BusinessEntityId == businessEntityId
                && x.PhoneNumber == phoneNumber
                && x.PhoneNumberTypeId == phoneNumberTypeId, ct);
        return row is null ? TypedResults.NotFound() : TypedResults.Ok(row.ToDto());
    }

    private static async Task<Results<Created<CompositeKeyResponse>, Conflict<string>, ValidationProblem>> CreateAsync(
        CreatePersonPhoneRequest request, IValidator<CreatePersonPhoneRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());

        var phone = (request.PhoneNumber ?? string.Empty).Trim();
        if (await db.PersonPhones.AnyAsync(x =>
                x.BusinessEntityId == request.BusinessEntityId
                && x.PhoneNumber == phone
                && x.PhoneNumberTypeId == request.PhoneNumberTypeId, ct))
        {
            return TypedResults.Conflict(
                $"Phone row ({request.BusinessEntityId}, {phone}, {request.PhoneNumberTypeId}) already exists.");
        }

        var entity = request.ToEntity();
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        db.PersonPhones.Add(entity);
        await db.SaveChangesAsync(ct);
        db.PersonPhoneAuditLogs.Add(PersonPhoneAuditService.RecordCreate(entity, user.Identity?.Name));
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return TypedResults.Created(
            $"/api/aw/person-phones/by-key?businessEntityId={entity.BusinessEntityId}&phoneNumber={Uri.EscapeDataString(entity.PhoneNumber)}&phoneNumberTypeId={entity.PhoneNumberTypeId}",
            new CompositeKeyResponse(new Dictionary<string, object>
            {
                ["businessEntityId"] = entity.BusinessEntityId,
                ["phoneNumber"] = entity.PhoneNumber,
                ["phoneNumberTypeId"] = entity.PhoneNumberTypeId,
            }));
    }

    private static async Task<Results<Ok<CompositeKeyResponse>, NotFound, ValidationProblem>> UpdateAsync(
        [FromQuery] int businessEntityId, [FromQuery] string phoneNumber, [FromQuery] int phoneNumberTypeId,
        UpdatePersonPhoneRequest request, IValidator<UpdatePersonPhoneRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());

        var entity = await db.PersonPhones
            .FirstOrDefaultAsync(x =>
                x.BusinessEntityId == businessEntityId
                && x.PhoneNumber == phoneNumber
                && x.PhoneNumberTypeId == phoneNumberTypeId, ct);
        if (entity is null) return TypedResults.NotFound();

        request.ApplyTo(entity);
        db.PersonPhoneAuditLogs.Add(PersonPhoneAuditService.RecordUpdate(entity, user.Identity?.Name));
        await db.SaveChangesAsync(ct);
        return TypedResults.Ok(new CompositeKeyResponse(new Dictionary<string, object>
        {
            ["businessEntityId"] = entity.BusinessEntityId,
            ["phoneNumber"] = entity.PhoneNumber,
            ["phoneNumberTypeId"] = entity.PhoneNumberTypeId,
        }));
    }

    private static async Task<Results<NoContent, NotFound>> DeleteAsync(
        [FromQuery] int businessEntityId, [FromQuery] string phoneNumber, [FromQuery] int phoneNumberTypeId,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var entity = await db.PersonPhones
            .FirstOrDefaultAsync(x =>
                x.BusinessEntityId == businessEntityId
                && x.PhoneNumber == phoneNumber
                && x.PhoneNumberTypeId == phoneNumberTypeId, ct);
        if (entity is null) return TypedResults.NotFound();

        db.PersonPhoneAuditLogs.Add(PersonPhoneAuditService.RecordDelete(entity, user.Identity?.Name));
        db.PersonPhones.Remove(entity);
        await db.SaveChangesAsync(ct);
        return TypedResults.NoContent();
    }

    private static async Task<Ok<List<PersonPhoneAuditLogDto>>> HistoryAsync(
        ApplicationDbContext db,
        [FromQuery] int? businessEntityId = null,
        [FromQuery] string? phoneNumber = null,
        [FromQuery] int? phoneNumberTypeId = null,
        CancellationToken ct = default)
    {
        var query = db.PersonPhoneAuditLogs.AsNoTracking();
        if (businessEntityId.HasValue) query = query.Where(a => a.BusinessEntityId == businessEntityId.Value);
        if (!string.IsNullOrWhiteSpace(phoneNumber)) query = query.Where(a => a.PhoneNumber == phoneNumber);
        if (phoneNumberTypeId.HasValue) query = query.Where(a => a.PhoneNumberTypeId == phoneNumberTypeId.Value);

        var rows = await query
            .OrderByDescending(a => a.ChangedDate).ThenByDescending(a => a.Id)
            .Select(a => a.ToDto())
            .ToListAsync(ct);
        return TypedResults.Ok(rows);
    }
}
