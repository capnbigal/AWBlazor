using System.Linq.Expressions;
using System.Security.Claims;
using AWBlazorApp.Features.Identity.Domain;
using AWBlazorApp.Features.Admin.Permissions.Domain;
using AWBlazorApp.Infrastructure.Persistence;
using AWBlazorApp.Shared.Audit;
using AWBlazorApp.Shared.Dtos;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Shared.Api;

/// <summary>
/// Simpler counterpart to <see cref="CrudEndpointBuilder.MapIntIdCrud"/> that relies on
/// <see cref="Persistence.AuditLogInterceptor"/> to emit audit rows automatically on
/// <c>SaveChangesAsync</c>. Callers no longer pass per-entity audit-service delegates
/// or <c>TAuditLog</c> types.
///
/// Map one of these per entity to land the five standard handlers (Get / Create / Update
/// / Delete / History) on a <see cref="RouteGroupBuilder"/>. List is still caller-owned
/// because every entity has bespoke query filters that belong in OpenAPI.
/// </summary>
public static class CrudWithInterceptorEndpointBuilder
{
    public static RouteGroupBuilder MapCrudWithInterceptor<TEntity, TDto, TCreate, TUpdate, TId>(
        this RouteGroupBuilder group,
        string entityName,
        string routePrefix,
        Func<ApplicationDbContext, DbSet<TEntity>> entitySet,
        Expression<Func<TEntity, TId>> idSelector,
        Func<TEntity, TId> getId,
        Func<TEntity, TDto> toDto,
        Func<TCreate, TEntity> toEntity,
        Action<TUpdate, TEntity> applyUpdate)
        where TEntity : class
        where TId : struct, IConvertible
    {
        group.MapGet("/{id:int}", async Task<Results<Ok<TDto>, NotFound>> (
            int id, ApplicationDbContext db, CancellationToken ct) =>
        {
            var predicate = BuildEquality(idSelector, id);
            var row = await entitySet(db).AsNoTracking().FirstOrDefaultAsync(predicate, ct);
            return row is null ? TypedResults.NotFound() : TypedResults.Ok(toDto(row));
        })
        .WithName($"Get{entityName}");

        group.MapPost("/", async Task<Results<Created<IdResponse>, ValidationProblem>> (
            TCreate request, IValidator<TCreate> validator,
            ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct) =>
        {
            var v = await validator.ValidateAsync(request, ct);
            if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());

            var entity = toEntity(request);
            entitySet(db).Add(entity);
            // Interceptor writes the audit row inside this SaveChangesAsync call.
            await db.SaveChangesAsync(ct);

            var newId = getId(entity);
            return TypedResults.Created($"{routePrefix}/{newId}", new IdResponse(newId!));
        })
        .WithName($"Create{entityName}")
        .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));

        group.MapPatch("/{id:int}", async Task<Results<Ok<IdResponse>, NotFound, ValidationProblem>> (
            int id, TUpdate request, IValidator<TUpdate> validator,
            ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct) =>
        {
            var v = await validator.ValidateAsync(request, ct);
            if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());

            var predicate = BuildEquality(idSelector, id);
            var entity = await entitySet(db).FirstOrDefaultAsync(predicate, ct);
            if (entity is null) return TypedResults.NotFound();

            applyUpdate(request, entity);
            // Interceptor diffs the ChangeTracker entries and writes the audit row.
            await db.SaveChangesAsync(ct);

            return TypedResults.Ok(new IdResponse(getId(entity)!));
        })
        .WithName($"Update{entityName}")
        .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));

        group.MapDelete("/{id:int}", async Task<Results<NoContent, NotFound>> (
            int id, ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct) =>
        {
            var predicate = BuildEquality(idSelector, id);
            var entity = await entitySet(db).FirstOrDefaultAsync(predicate, ct);
            if (entity is null) return TypedResults.NotFound();

            entitySet(db).Remove(entity);
            await db.SaveChangesAsync(ct);

            return TypedResults.NoContent();
        })
        .WithName($"Delete{entityName}")
        .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));

        // History: reads from the consolidated audit.AuditLog table filtered by EntityType
        // + EntityId. Returns a stable shape regardless of which entity is being queried.
        group.MapGet("/{id:int}/history", async Task<Ok<PagedResult<AuditLog>>> (
            int id, ApplicationDbContext db,
            [Microsoft.AspNetCore.Mvc.FromQuery] int skip,
            [Microsoft.AspNetCore.Mvc.FromQuery] int take,
            CancellationToken ct) =>
        {
            take = Math.Clamp(take == 0 ? 50 : take, 1, 1000);
            var idStr = id.ToString();
            var query = db.AuditLogs.AsNoTracking()
                .Where(a => a.EntityType == entityName && a.EntityId == idStr);
            var total = await query.CountAsync(ct);
            var rows = await query
                .OrderByDescending(a => a.ChangedDate).ThenByDescending(a => a.Id)
                .Skip(skip).Take(take)
                .ToListAsync(ct);
            return TypedResults.Ok(new PagedResult<AuditLog>(rows, total, skip, take));
        })
        .WithName($"List{entityName}History");

        return group;
    }

    // Builds `e => e.Id == id` for use in Where / FirstOrDefaultAsync when TId might be
    // byte / short / int / long. Convert.ChangeType casts the URL int to the entity's
    // actual key type so EF generates a correctly-typed parameter.
    private static Expression<Func<TEntity, bool>> BuildEquality<TEntity, TId>(
        Expression<Func<TEntity, TId>> idSelector, int id)
        where TId : struct, IConvertible
    {
        var converted = (TId)Convert.ChangeType(id, typeof(TId));
        var param = idSelector.Parameters[0];
        var equality = Expression.Equal(idSelector.Body, Expression.Constant(converted, typeof(TId)));
        return Expression.Lambda<Func<TEntity, bool>>(equality, param);
    }
}
