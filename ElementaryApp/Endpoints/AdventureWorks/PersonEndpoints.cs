using System.Security.Claims;
using ElementaryApp.Data;
using ElementaryApp.Models;
using ElementaryApp.Models.AdventureWorks;
using ElementaryApp.Services.AdventureWorksAudit;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ElementaryApp.Endpoints.AdventureWorks;

public static class PersonEndpoints
{
    public static IEndpointRouteBuilder MapPersonEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/aw/persons")
            .WithTags("Persons")
            .RequireAuthorization("ApiOrCookie");

        group.MapGet("/", ListAsync).WithName("ListPersons").WithSummary("List Person.Person rows.");
        group.MapGet("/{id:int}", GetAsync).WithName("GetPerson");
        group.MapPost("/", CreateAsync).WithName("CreatePerson")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));
        group.MapPatch("/{id:int}", UpdateAsync).WithName("UpdatePerson")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));
        group.MapDelete("/{id:int}", DeleteAsync).WithName("DeletePerson")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));
        group.MapGet("/{id:int}/history", HistoryAsync).WithName("ListPersonHistory");
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

    private static async Task<Results<Ok<PersonDto>, NotFound>> GetAsync(int id, ApplicationDbContext db, CancellationToken ct)
    {
        var row = await db.Persons.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        return row is null ? TypedResults.NotFound() : TypedResults.Ok(row.ToDto());
    }

    private static async Task<Results<Created<IdResponse>, Conflict<string>, ValidationProblem>> CreateAsync(
        CreatePersonRequest request, IValidator<CreatePersonRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());

        if (await db.Persons.AnyAsync(x => x.Id == request.Id, ct))
            return TypedResults.Conflict($"Person with BusinessEntityId {request.Id} already exists.");

        var entity = request.ToEntity();
        db.Persons.Add(entity);
        await db.SaveChangesAsync(ct);
        db.PersonAuditLogs.Add(PersonAuditService.RecordCreate(entity, user.Identity?.Name));
        await db.SaveChangesAsync(ct);
        return TypedResults.Created($"/api/aw/persons/{entity.Id}", new IdResponse(entity.Id));
    }

    private static async Task<Results<Ok<IdResponse>, NotFound, ValidationProblem>> UpdateAsync(
        int id, UpdatePersonRequest request, IValidator<UpdatePersonRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());

        var entity = await db.Persons.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null) return TypedResults.NotFound();

        var before = PersonAuditService.CaptureSnapshot(entity);
        request.ApplyTo(entity);
        db.PersonAuditLogs.Add(PersonAuditService.RecordUpdate(before, entity, user.Identity?.Name));
        await db.SaveChangesAsync(ct);
        return TypedResults.Ok(new IdResponse(entity.Id));
    }

    private static async Task<Results<NoContent, NotFound>> DeleteAsync(
        int id, ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var entity = await db.Persons.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null) return TypedResults.NotFound();

        db.PersonAuditLogs.Add(PersonAuditService.RecordDelete(entity, user.Identity?.Name));
        db.Persons.Remove(entity);
        await db.SaveChangesAsync(ct);
        return TypedResults.NoContent();
    }

    private static async Task<Ok<List<PersonAuditLogDto>>> HistoryAsync(int id, ApplicationDbContext db, CancellationToken ct)
    {
        var rows = await db.PersonAuditLogs.AsNoTracking()
            .Where(a => a.PersonId == id)
            .OrderByDescending(a => a.ChangedDate).ThenByDescending(a => a.Id)
            .Select(a => a.ToDto())
            .ToListAsync(ct);
        return TypedResults.Ok(rows);
    }
}
