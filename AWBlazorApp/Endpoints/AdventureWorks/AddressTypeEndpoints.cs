using System.Security.Claims;
using AWBlazorApp.Data;
using AWBlazorApp.Models;
using AWBlazorApp.Models.AdventureWorks;
using AWBlazorApp.Services.AdventureWorksAudit;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Endpoints.AdventureWorks;

public static class AddressTypeEndpoints
{
    public static IEndpointRouteBuilder MapAddressTypeEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/aw/address-types")
            .WithTags("AddressTypes")
            .RequireAuthorization("ApiOrCookie");

        group.MapGet("/", ListAsync)
            .WithName("ListAddressTypes")
            .WithSummary("List AdventureWorks Person.AddressType rows.");

        group.MapGet("/{id:int}", GetAsync)
            .WithName("GetAddressType")
            .WithSummary("Get a single AddressType by id.");

        group.MapPost("/", CreateAsync)
            .WithName("CreateAddressType")
            .WithSummary("Create a new AddressType. Requires Employee role.")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));

        group.MapPatch("/{id:int}", UpdateAsync)
            .WithName("UpdateAddressType")
            .WithSummary("Update an AddressType. Requires Employee role.")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));

        group.MapDelete("/{id:int}", DeleteAsync)
            .WithName("DeleteAddressType")
            .WithSummary("Delete an AddressType. Requires Manager role.")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));

        group.MapGet("/{id:int}/history", HistoryAsync)
            .WithName("ListAddressTypeHistory")
            .WithSummary("List audit-history rows for a single AddressType id.");

        return app;
    }

    private static async Task<Ok<PagedResult<AddressTypeDto>>> ListAsync(
        ApplicationDbContext db,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 50,
        [FromQuery] string? name = null,
        CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 1000);
        var query = db.AddressTypes.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(name)) query = query.Where(x => x.Name.Contains(name));

        var total = await query.CountAsync(ct);
        var rows = await query.OrderBy(x => x.Id)
            .Skip(skip).Take(take)
            .Select(x => x.ToDto())
            .ToListAsync(ct);

        return TypedResults.Ok(new PagedResult<AddressTypeDto>(rows, total, skip, take));
    }

    private static async Task<Results<Ok<AddressTypeDto>, NotFound>> GetAsync(
        int id, ApplicationDbContext db, CancellationToken ct)
    {
        var row = await db.AddressTypes.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        return row is null ? TypedResults.NotFound() : TypedResults.Ok(row.ToDto());
    }

    private static async Task<Results<Created<IdResponse>, ValidationProblem>> CreateAsync(
        CreateAddressTypeRequest request,
        IValidator<CreateAddressTypeRequest> validator,
        ApplicationDbContext db,
        ClaimsPrincipal user,
        CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());

        var entity = request.ToEntity();
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        db.AddressTypes.Add(entity);
        await db.SaveChangesAsync(ct);

        db.AddressTypeAuditLogs.Add(AddressTypeAuditService.RecordCreate(entity, user.Identity?.Name));
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        return TypedResults.Created($"/api/aw/address-types/{entity.Id}", new IdResponse(entity.Id));
    }

    private static async Task<Results<Ok<IdResponse>, NotFound, ValidationProblem>> UpdateAsync(
        int id,
        UpdateAddressTypeRequest request,
        IValidator<UpdateAddressTypeRequest> validator,
        ApplicationDbContext db,
        ClaimsPrincipal user,
        CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());

        var entity = await db.AddressTypes.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null) return TypedResults.NotFound();

        var before = AddressTypeAuditService.CaptureSnapshot(entity);
        request.ApplyTo(entity);
        db.AddressTypeAuditLogs.Add(AddressTypeAuditService.RecordUpdate(before, entity, user.Identity?.Name));
        await db.SaveChangesAsync(ct);

        return TypedResults.Ok(new IdResponse(entity.Id));
    }

    private static async Task<Results<NoContent, NotFound>> DeleteAsync(
        int id, ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var entity = await db.AddressTypes.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null) return TypedResults.NotFound();

        db.AddressTypeAuditLogs.Add(AddressTypeAuditService.RecordDelete(entity, user.Identity?.Name));
        db.AddressTypes.Remove(entity);
        await db.SaveChangesAsync(ct);
        return TypedResults.NoContent();
    }

    private static async Task<Ok<List<AddressTypeAuditLogDto>>> HistoryAsync(
        int id, ApplicationDbContext db, CancellationToken ct)
    {
        var rows = await db.AddressTypeAuditLogs.AsNoTracking()
            .Where(a => a.AddressTypeId == id)
            .OrderByDescending(a => a.ChangedDate).ThenByDescending(a => a.Id)
            .Select(a => a.ToDto())
            .ToListAsync(ct);
        return TypedResults.Ok(rows);
    }
}
