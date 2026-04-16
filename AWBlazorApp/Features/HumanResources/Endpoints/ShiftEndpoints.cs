using System.Security.Claims;
using AWBlazorApp.Data;
using AWBlazorApp.Infrastructure.Persistence;
using AWBlazorApp.Shared.Models;
using AWBlazorApp.Features.HumanResources.Models;
using AWBlazorApp.Features.HumanResources.Audit;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.HumanResources.Endpoints;

public static class ShiftEndpoints
{
    public static IEndpointRouteBuilder MapShiftEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/aw/shifts")
            .WithTags("Shifts")
            .RequireAuthorization("ApiOrCookie");

        group.MapGet("/", ListAsync).WithName("ListShifts").WithSummary("List HumanResources.Shift rows.");
        group.MapGet("/{id:int}", GetAsync).WithName("GetShift");
        group.MapPost("/", CreateAsync).WithName("CreateShift")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));
        group.MapPatch("/{id:int}", UpdateAsync).WithName("UpdateShift")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));
        group.MapDelete("/{id:int}", DeleteAsync).WithName("DeleteShift")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));
        group.MapGet("/{id:int}/history", HistoryAsync).WithName("ListShiftHistory");
        return app;
    }

    private static async Task<Ok<PagedResult<ShiftDto>>> ListAsync(
        ApplicationDbContext db, [FromQuery] int skip = 0, [FromQuery] int take = 50,
        [FromQuery] string? name = null, CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 1000);
        var query = db.Shifts.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(name)) query = query.Where(x => x.Name.Contains(name));
        var total = await query.CountAsync(ct);
        var rows = await query.OrderBy(x => x.Id).Skip(skip).Take(take).Select(x => x.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<ShiftDto>(rows, total, skip, take));
    }

    private static async Task<Results<Ok<ShiftDto>, NotFound>> GetAsync(int id, ApplicationDbContext db, CancellationToken ct)
    {
        var key = (byte)id;
        var row = await db.Shifts.AsNoTracking().FirstOrDefaultAsync(x => x.Id == key, ct);
        return row is null ? TypedResults.NotFound() : TypedResults.Ok(row.ToDto());
    }

    private static async Task<Results<Created<IdResponse>, ValidationProblem>> CreateAsync(
        CreateShiftRequest request, IValidator<CreateShiftRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());

        var entity = request.ToEntity();
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        db.Shifts.Add(entity);
        await db.SaveChangesAsync(ct);
        db.ShiftAuditLogs.Add(ShiftAuditService.RecordCreate(entity, user.Identity?.Name));
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return TypedResults.Created($"/api/aw/shifts/{entity.Id}", new IdResponse(entity.Id));
    }

    private static async Task<Results<Ok<IdResponse>, NotFound, ValidationProblem>> UpdateAsync(
        int id, UpdateShiftRequest request, IValidator<UpdateShiftRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());

        var key = (byte)id;
        var entity = await db.Shifts.FirstOrDefaultAsync(x => x.Id == key, ct);
        if (entity is null) return TypedResults.NotFound();

        var before = ShiftAuditService.CaptureSnapshot(entity);
        request.ApplyTo(entity);
        db.ShiftAuditLogs.Add(ShiftAuditService.RecordUpdate(before, entity, user.Identity?.Name));
        await db.SaveChangesAsync(ct);
        return TypedResults.Ok(new IdResponse(entity.Id));
    }

    private static async Task<Results<NoContent, NotFound>> DeleteAsync(
        int id, ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var key = (byte)id;
        var entity = await db.Shifts.FirstOrDefaultAsync(x => x.Id == key, ct);
        if (entity is null) return TypedResults.NotFound();

        db.ShiftAuditLogs.Add(ShiftAuditService.RecordDelete(entity, user.Identity?.Name));
        db.Shifts.Remove(entity);
        await db.SaveChangesAsync(ct);
        return TypedResults.NoContent();
    }

    private static async Task<Ok<List<ShiftAuditLogDto>>> HistoryAsync(int id, ApplicationDbContext db, CancellationToken ct)
    {
        var key = (byte)id;
        var rows = await db.ShiftAuditLogs.AsNoTracking()
            .Where(a => a.ShiftId == key)
            .OrderByDescending(a => a.ChangedDate).ThenByDescending(a => a.Id)
            .Select(a => a.ToDto())
            .ToListAsync(ct);
        return TypedResults.Ok(rows);
    }
}
