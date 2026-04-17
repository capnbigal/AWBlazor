using System.Linq.Expressions;
using System.Security.Claims;
using AWBlazorApp.Data;
using AWBlazorApp.Infrastructure.Persistence;
using AWBlazorApp.Shared.Models;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Shared.Endpoints;

/// <summary>
/// Maps the five identical CRUD-audit handlers (Get / Create / Update / Delete / History) onto a
/// RouteGroupBuilder. List is intentionally left to the caller because every entity has
/// entity-specific query filters that must surface in OpenAPI.
///
/// Call this from a feature's Map…Endpoints extension AFTER setting up the group with
/// MapGroup + WithTags + RequireAuthorization("ApiOrCookie"), then chain the caller's own
/// ListAsync registration.
///
/// TId may be int / short / byte — the URL parameter is always <c>int</c> (matching the route
/// constraint <c>{id:int}</c>); if TId is narrower it is converted via <see cref="Convert.ChangeType(object, Type)"/>
/// when the EF predicate is built.
/// </summary>
public static class CrudEndpointBuilder
{
    public static RouteGroupBuilder MapIntIdCrud<TEntity, TDto, TCreate, TUpdate, TAuditLog, TAuditDto, TSnapshot, TId>(
        this RouteGroupBuilder group,
        string entityName,
        string routePrefix,
        Func<ApplicationDbContext, DbSet<TEntity>> entitySet,
        Func<ApplicationDbContext, DbSet<TAuditLog>> auditSet,
        Expression<Func<TEntity, TId>> idSelector,
        Expression<Func<TAuditLog, TId>> auditIdSelector,
        Expression<Func<TAuditLog, DateTime>> auditChangedDateSelector,
        Expression<Func<TAuditLog, int>> auditPrimaryKeySelector,
        Func<TEntity, TId> getId,
        Func<TEntity, TDto> toDto,
        Func<TCreate, TEntity> toEntity,
        Action<TUpdate, TEntity> applyUpdate,
        Func<TEntity, TSnapshot> captureSnapshot,
        Func<TEntity, string?, TAuditLog> recordCreate,
        Func<TSnapshot, TEntity, string?, TAuditLog> recordUpdate,
        Func<TEntity, string?, TAuditLog> recordDelete,
        Func<TAuditLog, TAuditDto> auditToDto)
        where TEntity : class
        where TAuditLog : class
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
            await using var tx = await db.Database.BeginTransactionAsync(ct);
            entitySet(db).Add(entity);
            await db.SaveChangesAsync(ct);
            auditSet(db).Add(recordCreate(entity, user.Identity?.Name));
            await db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
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

            var before = captureSnapshot(entity);
            applyUpdate(request, entity);
            auditSet(db).Add(recordUpdate(before, entity, user.Identity?.Name));
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

            auditSet(db).Add(recordDelete(entity, user.Identity?.Name));
            entitySet(db).Remove(entity);
            await db.SaveChangesAsync(ct);
            return TypedResults.NoContent();
        })
        .WithName($"Delete{entityName}")
        .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));

        group.MapGet("/{id:int}/history", async Task<Ok<List<TAuditDto>>> (
            int id, ApplicationDbContext db, CancellationToken ct) =>
        {
            var predicate = BuildEquality(auditIdSelector, id);
            var rows = await auditSet(db).AsNoTracking()
                .Where(predicate)
                .OrderByDescending(auditChangedDateSelector)
                .ThenByDescending(auditPrimaryKeySelector)
                .Select(a => auditToDto(a))
                .ToListAsync(ct);
            return TypedResults.Ok(rows);
        })
        .WithName($"List{entityName}History");

        return group;
    }

    private static Expression<Func<T, bool>> BuildEquality<T, TValue>(Expression<Func<T, TValue>> selector, int value)
        where TValue : struct, IConvertible
    {
        var param = selector.Parameters[0];
        var typedValue = (TValue)Convert.ChangeType(value, typeof(TValue));
        var valueExpr = Expression.Constant(typedValue, typeof(TValue));
        var body = Expression.Equal(selector.Body, valueExpr);
        return Expression.Lambda<Func<T, bool>>(body, param);
    }
}
