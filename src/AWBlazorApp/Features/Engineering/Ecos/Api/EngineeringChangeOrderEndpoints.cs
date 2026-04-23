using System.Security.Claims;
using AWBlazorApp.Features.Identity.Domain; using AWBlazorApp.Features.Admin.Permissions.Domain;
using AWBlazorApp.Features.Engineering.Ecos.Domain;
using AWBlazorApp.Features.Engineering.Ecos.Dtos;
using AWBlazorApp.Features.Engineering.Ecos.Application.Services;
using AWBlazorApp.Infrastructure.Persistence;
using AWBlazorApp.Shared.Audit;
using AWBlazorApp.Shared.Dtos;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.Engineering.Ecos.Api;

public static class EngineeringChangeOrderEndpoints
{
    public static IEndpointRouteBuilder MapEngineeringChangeOrderEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/engineering-change-orders")
            .WithTags("EngineeringChangeOrders")
            .RequireAuthorization("ApiOrCookie");

        group.MapGet("/", ListAsync).WithName("ListEcos");
        group.MapGet("/{id:int}", GetAsync).WithName("GetEco");
        group.MapPost("/", CreateAsync).WithName("CreateEco");
        group.MapPatch("/{id:int}", UpdateAsync).WithName("UpdateEco");
        group.MapDelete("/{id:int}", DeleteAsync).WithName("DeleteEco")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Admin));
        group.MapGet("/{id:int}/history", HistoryAsync).WithName("ListEcoHistory");

        group.MapPost("/{id:int}/submit", SubmitAsync).WithName("SubmitEco");
        group.MapPost("/{id:int}/approve", ApproveAsync).WithName("ApproveEco")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));
        group.MapPost("/{id:int}/reject", RejectAsync).WithName("RejectEco")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));
        group.MapPost("/{id:int}/cancel", CancelAsync).WithName("CancelEco");

        group.MapGet("/{id:int}/affected-items", ListAffectedAsync).WithName("ListEcoAffectedItems");
        group.MapPost("/{id:int}/affected-items", CreateAffectedAsync).WithName("CreateEcoAffectedItem");
        group.MapDelete("/affected-items/{aId:int}", DeleteAffectedAsync).WithName("DeleteEcoAffectedItem");

        group.MapGet("/{id:int}/approvals", ListApprovalsAsync).WithName("ListEcoApprovals");

        return app;
    }

    private static async Task<Ok<PagedResult<EngineeringChangeOrderDto>>> ListAsync(
        ApplicationDbContext db,
        [FromQuery] int skip = 0, [FromQuery] int take = 50,
        [FromQuery] EcoStatus? status = null,
        CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 500);
        var q = db.EngineeringChangeOrders.AsNoTracking();
        if (status.HasValue) q = q.Where(x => x.Status == status.Value);
        var total = await q.CountAsync(ct);
        var rows = await q.OrderByDescending(x => x.RaisedAt)
            .Skip(skip).Take(take).Select(x => x.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<EngineeringChangeOrderDto>(rows, total, skip, take));
    }

    private static async Task<Results<Ok<EngineeringChangeOrderDto>, NotFound>> GetAsync(int id, ApplicationDbContext db, CancellationToken ct)
    {
        var row = await db.EngineeringChangeOrders.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        return row is null ? TypedResults.NotFound() : TypedResults.Ok(row.ToDto());
    }

    private static async Task<Results<Created<IdResponse>, ValidationProblem>> CreateAsync(
        CreateEngineeringChangeOrderRequest request,
        IValidator<CreateEngineeringChangeOrderRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());
        var entity = request.ToEntity(user.Identity?.Name);
        db.EngineeringChangeOrders.Add(entity);
        // AuditLogInterceptor writes the audit row inside this SaveChangesAsync.
        await db.SaveChangesAsync(ct);
        return TypedResults.Created($"/api/engineering-change-orders/{entity.Id}", new IdResponse(entity.Id));
    }

    private static async Task<Results<Ok<IdResponse>, NotFound, ValidationProblem, BadRequest<string>>> UpdateAsync(
        int id, UpdateEngineeringChangeOrderRequest request,
        IValidator<UpdateEngineeringChangeOrderRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());
        var entity = await db.EngineeringChangeOrders.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null) return TypedResults.NotFound();
        if (entity.Status != EcoStatus.Draft)
            return TypedResults.BadRequest("ECO can only be edited while in Draft status.");
        request.ApplyTo(entity);
        await db.SaveChangesAsync(ct);
        return TypedResults.Ok(new IdResponse(entity.Id));
    }

    private static async Task<Results<NoContent, NotFound>> DeleteAsync(
        int id, ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var entity = await db.EngineeringChangeOrders.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null) return TypedResults.NotFound();
        db.EngineeringChangeOrders.Remove(entity);
        await db.SaveChangesAsync(ct);
        return TypedResults.NoContent();
    }

    private static async Task<Ok<PagedResult<AuditLog>>> HistoryAsync(
        int id, ApplicationDbContext db,
        [FromQuery] int skip = 0, [FromQuery] int take = 50, CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 500);
        var idStr = id.ToString();
        var q = db.AuditLogs.AsNoTracking().Where(a => a.EntityType == "EngineeringChangeOrder" && a.EntityId == idStr);
        var total = await q.CountAsync(ct);
        var rows = await q.OrderByDescending(a => a.ChangedDate).ThenByDescending(a => a.Id)
            .Skip(skip).Take(take).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<AuditLog>(rows, total, skip, take));
    }

    private static async Task<Results<NoContent, NotFound, BadRequest<string>>> SubmitAsync(
        int id, IEcoService svc, ClaimsPrincipal user, CancellationToken ct)
        => await TransitionAsync(() => svc.SubmitForReviewAsync(id, user.Identity?.Name, ct));

    private static async Task<Results<NoContent, NotFound, BadRequest<string>>> ApproveAsync(
        int id, ReviewEcoRequest request, IEcoService svc, ClaimsPrincipal user, CancellationToken ct)
        => await TransitionAsync(() => svc.ApproveAsync(id, request.Notes, user.Identity?.Name, ct));

    private static async Task<Results<NoContent, NotFound, BadRequest<string>>> RejectAsync(
        int id, ReviewEcoRequest request, IEcoService svc, ClaimsPrincipal user, CancellationToken ct)
        => await TransitionAsync(() => svc.RejectAsync(id, request.Notes, user.Identity?.Name, ct));

    private static async Task<Results<NoContent, NotFound, BadRequest<string>>> CancelAsync(
        int id, IEcoService svc, ClaimsPrincipal user, CancellationToken ct)
        => await TransitionAsync(() => svc.CancelAsync(id, user.Identity?.Name, ct));

    private static async Task<Results<NoContent, NotFound, BadRequest<string>>> TransitionAsync(Func<Task> action)
    {
        try
        {
            await action();
            return TypedResults.NoContent();
        }
        catch (KeyNotFoundException) { return TypedResults.NotFound(); }
        catch (InvalidOperationException ex) { return TypedResults.BadRequest(ex.Message); }
    }

    private static async Task<Ok<PagedResult<EcoAffectedItemDto>>> ListAffectedAsync(
        int id, ApplicationDbContext db, CancellationToken ct = default)
    {
        var rows = await db.EcoAffectedItems.AsNoTracking()
            .Where(a => a.EngineeringChangeOrderId == id)
            .OrderBy(a => a.AffectedKind).ThenBy(a => a.Id)
            .Select(a => a.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<EcoAffectedItemDto>(rows, rows.Count, 0, rows.Count));
    }

    private static async Task<Results<Created<IdResponse>, NotFound, BadRequest<string>, ValidationProblem>> CreateAffectedAsync(
        int id, CreateEcoAffectedItemRequest request,
        IValidator<CreateEcoAffectedItemRequest> validator,
        ApplicationDbContext db, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());
        var parent = await db.EngineeringChangeOrders.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id, ct);
        if (parent is null) return TypedResults.NotFound();
        if (parent.Status != EcoStatus.Draft)
            return TypedResults.BadRequest("Affected items can only be added while the ECO is in Draft status.");
        var entity = request.ToEntity(id);
        db.EcoAffectedItems.Add(entity);
        await db.SaveChangesAsync(ct);
        return TypedResults.Created($"/api/engineering-change-orders/{id}/affected-items/{entity.Id}", new IdResponse(entity.Id));
    }

    private static async Task<Results<NoContent, NotFound>> DeleteAffectedAsync(
        int aId, ApplicationDbContext db, CancellationToken ct)
    {
        var entity = await db.EcoAffectedItems.FirstOrDefaultAsync(a => a.Id == aId, ct);
        if (entity is null) return TypedResults.NotFound();
        db.EcoAffectedItems.Remove(entity);
        await db.SaveChangesAsync(ct);
        return TypedResults.NoContent();
    }

    private static async Task<Ok<PagedResult<EcoApprovalDto>>> ListApprovalsAsync(
        int id, ApplicationDbContext db, CancellationToken ct = default)
    {
        var rows = await db.EcoApprovals.AsNoTracking()
            .Where(a => a.EngineeringChangeOrderId == id)
            .OrderByDescending(a => a.DecidedAt)
            .Select(a => a.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<EcoApprovalDto>(rows, rows.Count, 0, rows.Count));
    }
}
