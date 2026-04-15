using System.Security.Claims;
using AWBlazorApp.Data;
using AWBlazorApp.Infrastructure.Persistence;
using AWBlazorApp.Models;
using AWBlazorApp.Services;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Endpoints;

public static class ToolSlotConfigurationEndpoints
{
    public static IEndpointRouteBuilder MapToolSlotConfigurationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/tool-slots")
            .WithTags("ToolSlotConfigurations")
            .RequireAuthorization("ApiOrCookie");

        group.MapGet("/", ListAsync)
            .WithName("ListToolSlotConfigurations")
            .WithSummary("List tool-slot configurations.");

        group.MapGet("/{id:int}", GetAsync)
            .WithName("GetToolSlotConfiguration")
            .WithSummary("Get a single tool-slot configuration by id.");

        group.MapPost("/", CreateAsync)
            .WithName("CreateToolSlotConfiguration")
            .WithSummary("Create a new tool-slot configuration. Requires Employee role.")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));

        group.MapPatch("/{id:int}", UpdateAsync)
            .WithName("UpdateToolSlotConfiguration")
            .WithSummary("Update a tool-slot configuration. Requires Employee role.")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));

        group.MapDelete("/{id:int}", DeleteAsync)
            .WithName("DeleteToolSlotConfiguration")
            .WithSummary("Delete a tool-slot configuration. Requires Manager role.")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));

        return app;
    }

    private static async Task<Ok<PagedResult<ToolSlotConfigurationDto>>> ListAsync(
        ApplicationDbContext db,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 50,
        [FromQuery] string? family = null,
        [FromQuery] string? mtCode = null,
        [FromQuery] string? destination = null,
        [FromQuery] bool? isActive = null,
        CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 1000);

        var query = db.ToolSlotConfigurations.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(family)) query = query.Where(t => t.Family == family);
        if (!string.IsNullOrWhiteSpace(mtCode)) query = query.Where(t => t.MtCode == mtCode);
        if (!string.IsNullOrWhiteSpace(destination)) query = query.Where(t => t.Destination == destination);
        if (isActive.HasValue) query = query.Where(t => t.IsActive == isActive.Value);

        var total = await query.CountAsync(ct);
        var rows = await query
            .OrderBy(t => t.Id)
            .Skip(skip)
            .Take(take)
            .Select(t => t.ToDto())
            .ToListAsync(ct);

        return TypedResults.Ok(new PagedResult<ToolSlotConfigurationDto>(rows, total, skip, take));
    }

    private static async Task<Results<Ok<ToolSlotConfigurationDto>, NotFound>> GetAsync(
        int id, ApplicationDbContext db, CancellationToken ct)
    {
        var slot = await db.ToolSlotConfigurations.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id, ct);
        return slot is null ? TypedResults.NotFound() : TypedResults.Ok(slot.ToDto());
    }

    private static async Task<Results<Created<IdResponse>, ValidationProblem>> CreateAsync(
        CreateToolSlotConfigurationRequest request,
        IValidator<CreateToolSlotConfigurationRequest> validator,
        ApplicationDbContext db,
        ClaimsPrincipal user,
        CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            return TypedResults.ValidationProblem(validation.ToDictionary());
        }

        var entity = request.ToEntity();
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        db.ToolSlotConfigurations.Add(entity);
        await db.SaveChangesAsync(ct);

        db.ToolSlotAuditLogs.Add(ToolSlotAuditService.RecordCreate(entity, user.Identity?.Name));
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        return TypedResults.Created($"/api/tool-slots/{entity.Id}", new IdResponse(entity.Id));
    }

    private static async Task<Results<Ok<IdResponse>, NotFound, ValidationProblem>> UpdateAsync(
        int id,
        UpdateToolSlotConfigurationRequest request,
        IValidator<UpdateToolSlotConfigurationRequest> validator,
        ApplicationDbContext db,
        ClaimsPrincipal user,
        CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            return TypedResults.ValidationProblem(validation.ToDictionary());
        }

        var entity = await db.ToolSlotConfigurations.FirstOrDefaultAsync(t => t.Id == id, ct);
        if (entity is null) return TypedResults.NotFound();

        var before = ToolSlotAuditService.Snapshot(entity);
        request.ApplyTo(entity);
        db.ToolSlotAuditLogs.Add(ToolSlotAuditService.RecordUpdate(before, entity, user.Identity?.Name));
        await db.SaveChangesAsync(ct);

        return TypedResults.Ok(new IdResponse(entity.Id));
    }

    private static async Task<Results<NoContent, NotFound>> DeleteAsync(
        int id, ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var entity = await db.ToolSlotConfigurations.FirstOrDefaultAsync(t => t.Id == id, ct);
        if (entity is null) return TypedResults.NotFound();

        db.ToolSlotAuditLogs.Add(ToolSlotAuditService.RecordDelete(entity, user.Identity?.Name));
        db.ToolSlotConfigurations.Remove(entity);
        await db.SaveChangesAsync(ct);

        return TypedResults.NoContent();
    }
}
