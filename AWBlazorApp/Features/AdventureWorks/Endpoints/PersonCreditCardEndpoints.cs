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

public static class PersonCreditCardEndpoints
{
    public static IEndpointRouteBuilder MapPersonCreditCardEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/aw/person-credit-cards")
            .WithTags("PersonCreditCards")
            .RequireAuthorization("ApiOrCookie");

        group.MapGet("/", ListAsync).WithName("ListPersonCreditCards")
            .WithSummary("List Sales.PersonCreditCard rows. Composite PK = (BusinessEntityID, CreditCardID).");
        group.MapGet("/by-key", GetAsync).WithName("GetPersonCreditCard");
        group.MapPost("/", CreateAsync).WithName("CreatePersonCreditCard")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));
        group.MapPatch("/by-key", UpdateAsync).WithName("UpdatePersonCreditCard")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));
        group.MapDelete("/by-key", DeleteAsync).WithName("DeletePersonCreditCard")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));
        group.MapGet("/history", HistoryAsync).WithName("ListPersonCreditCardHistory");
        return app;
    }

    private static async Task<Ok<PagedResult<PersonCreditCardDto>>> ListAsync(
        ApplicationDbContext db, [FromQuery] int skip = 0, [FromQuery] int take = 50,
        [FromQuery] int? businessEntityId = null, CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 1000);
        var query = db.PersonCreditCards.AsNoTracking();
        if (businessEntityId.HasValue) query = query.Where(x => x.BusinessEntityId == businessEntityId.Value);
        var total = await query.CountAsync(ct);
        var rows = await query.OrderBy(x => x.BusinessEntityId).ThenBy(x => x.CreditCardId)
            .Skip(skip).Take(take).Select(x => x.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<PersonCreditCardDto>(rows, total, skip, take));
    }

    private static async Task<Results<Ok<PersonCreditCardDto>, NotFound>> GetAsync(
        [FromQuery] int businessEntityId, [FromQuery] int creditCardId,
        ApplicationDbContext db, CancellationToken ct)
    {
        var row = await db.PersonCreditCards.AsNoTracking()
            .FirstOrDefaultAsync(x => x.BusinessEntityId == businessEntityId && x.CreditCardId == creditCardId, ct);
        return row is null ? TypedResults.NotFound() : TypedResults.Ok(row.ToDto());
    }

    private static async Task<Results<Created<CompositeKeyResponse>, Conflict<string>, ValidationProblem>> CreateAsync(
        CreatePersonCreditCardRequest request, IValidator<CreatePersonCreditCardRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());

        if (await db.PersonCreditCards.AnyAsync(x =>
                x.BusinessEntityId == request.BusinessEntityId && x.CreditCardId == request.CreditCardId, ct))
        {
            return TypedResults.Conflict($"Junction row ({request.BusinessEntityId}, {request.CreditCardId}) already exists.");
        }

        var entity = request.ToEntity();
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        db.PersonCreditCards.Add(entity);
        await db.SaveChangesAsync(ct);
        db.PersonCreditCardAuditLogs.Add(PersonCreditCardAuditService.RecordCreate(entity, user.Identity?.Name));
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return TypedResults.Created(
            $"/api/aw/person-credit-cards/by-key?businessEntityId={entity.BusinessEntityId}&creditCardId={entity.CreditCardId}",
            new CompositeKeyResponse(new Dictionary<string, object>
            {
                ["businessEntityId"] = entity.BusinessEntityId,
                ["creditCardId"] = entity.CreditCardId,
            }));
    }

    private static async Task<Results<Ok<CompositeKeyResponse>, NotFound, ValidationProblem>> UpdateAsync(
        [FromQuery] int businessEntityId, [FromQuery] int creditCardId,
        UpdatePersonCreditCardRequest request, IValidator<UpdatePersonCreditCardRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());

        var entity = await db.PersonCreditCards
            .FirstOrDefaultAsync(x => x.BusinessEntityId == businessEntityId && x.CreditCardId == creditCardId, ct);
        if (entity is null) return TypedResults.NotFound();

        request.ApplyTo(entity);
        db.PersonCreditCardAuditLogs.Add(PersonCreditCardAuditService.RecordUpdate(entity, user.Identity?.Name));
        await db.SaveChangesAsync(ct);
        return TypedResults.Ok(new CompositeKeyResponse(new Dictionary<string, object>
        {
            ["businessEntityId"] = entity.BusinessEntityId,
            ["creditCardId"] = entity.CreditCardId,
        }));
    }

    private static async Task<Results<NoContent, NotFound>> DeleteAsync(
        [FromQuery] int businessEntityId, [FromQuery] int creditCardId,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var entity = await db.PersonCreditCards
            .FirstOrDefaultAsync(x => x.BusinessEntityId == businessEntityId && x.CreditCardId == creditCardId, ct);
        if (entity is null) return TypedResults.NotFound();

        db.PersonCreditCardAuditLogs.Add(PersonCreditCardAuditService.RecordDelete(entity, user.Identity?.Name));
        db.PersonCreditCards.Remove(entity);
        await db.SaveChangesAsync(ct);
        return TypedResults.NoContent();
    }

    private static async Task<Ok<List<PersonCreditCardAuditLogDto>>> HistoryAsync(
        ApplicationDbContext db,
        [FromQuery] int? businessEntityId = null,
        [FromQuery] int? creditCardId = null,
        CancellationToken ct = default)
    {
        var query = db.PersonCreditCardAuditLogs.AsNoTracking();
        if (businessEntityId.HasValue) query = query.Where(a => a.BusinessEntityId == businessEntityId.Value);
        if (creditCardId.HasValue) query = query.Where(a => a.CreditCardId == creditCardId.Value);

        var rows = await query
            .OrderByDescending(a => a.ChangedDate).ThenByDescending(a => a.Id)
            .Select(a => a.ToDto())
            .ToListAsync(ct);
        return TypedResults.Ok(rows);
    }
}
