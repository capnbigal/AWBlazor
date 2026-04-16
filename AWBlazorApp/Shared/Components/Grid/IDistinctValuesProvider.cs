using System.Collections;
using System.Linq.Expressions;
using System.Reflection;
using AWBlazorApp.Data;
using AWBlazorApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Shared.Components.Grid;

public interface IDistinctValuesProvider
{
    /// <summary>
    /// Returns distinct values of <paramref name="propertyName"/> from the backing DbSet of
    /// <typeparamref name="TEntity"/>, filtered by <paramref name="search"/> (case-insensitive
    /// substring on the stringified value) and capped at <paramref name="take"/>. Values come back
    /// ordered ascending.
    /// </summary>
    Task<List<object?>> GetDistinctValuesAsync<TEntity>(
        string propertyName,
        string? search,
        int take,
        CancellationToken ct) where TEntity : class;
}

public sealed class DistinctValuesProvider(IDbContextFactory<ApplicationDbContext> dbFactory) : IDistinctValuesProvider
{
    public async Task<List<object?>> GetDistinctValuesAsync<TEntity>(
        string propertyName, string? search, int take, CancellationToken ct) where TEntity : class
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var property = typeof(TEntity).GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance)
            ?? throw new InvalidOperationException($"{typeof(TEntity).Name} has no property '{propertyName}'.");

        var query = db.Set<TEntity>().AsNoTracking().AsQueryable();

        // Project x => x.Prop via reflection-built Expression so EF translates to SELECT DISTINCT.
        var parameter = Expression.Parameter(typeof(TEntity), "x");
        var propertyAccess = Expression.Property(parameter, property);
        var selectLambda = Expression.Lambda(propertyAccess, parameter);

        var selectMethod = typeof(Queryable).GetMethods()
            .First(m => m.Name == nameof(Queryable.Select)
                && m.GetParameters()[1].ParameterType.GetGenericArguments()[0].GetGenericArguments().Length == 2)
            .MakeGenericMethod(typeof(TEntity), property.PropertyType);

        var projected = (IQueryable)selectMethod.Invoke(null, new object[] { query, selectLambda })!;

        // DISTINCT + ORDER BY + TAKE, all typed to the property's type.
        var distinctMethod = typeof(Queryable).GetMethods()
            .First(m => m.Name == nameof(Queryable.Distinct) && m.GetParameters().Length == 1)
            .MakeGenericMethod(property.PropertyType);
        projected = (IQueryable)distinctMethod.Invoke(null, new object[] { projected })!;

        var orderByMethod = typeof(Queryable).GetMethods()
            .First(m => m.Name == nameof(Queryable.OrderBy) && m.GetParameters().Length == 2)
            .MakeGenericMethod(property.PropertyType, property.PropertyType);
        var identityParam = Expression.Parameter(property.PropertyType, "v");
        var identityLambda = Expression.Lambda(identityParam, identityParam);
        projected = (IQueryable)orderByMethod.Invoke(null, new object[] { projected, identityLambda })!;

        // Materialise. Search filter is applied client-side (after DISTINCT) because building a
        // SQL substring filter on non-string columns is awkward; DISTINCT already caps the
        // cardinality and the take limit bounds the rest.
        // Use the GetMethods/.First pattern (same as Select/Distinct/OrderBy above) because
        // GetMethod(name, Type[]) can't match open generic signatures like Take<TSource>(IQueryable<TSource>, int);
        // it also needs to disambiguate from the .NET 6+ Take(IQueryable<T>, Range) overload.
        var takeMethod = typeof(Queryable).GetMethods()
            .First(m => m.Name == nameof(Queryable.Take)
                && m.GetParameters().Length == 2
                && m.GetParameters()[1].ParameterType == typeof(int))
            .MakeGenericMethod(property.PropertyType);
        projected = (IQueryable)takeMethod.Invoke(null, new object[] { projected, Math.Max(take, 1) * 4 })!;

        var list = new List<object?>();
        foreach (var v in projected)
            list.Add(v);

        IEnumerable<object?> filtered = list;
        if (!string.IsNullOrWhiteSpace(search))
        {
            var needle = search.Trim();
            filtered = list.Where(v => v?.ToString()?.Contains(needle, StringComparison.OrdinalIgnoreCase) == true);
        }

        await Task.CompletedTask;
        return filtered.Take(take).ToList();
    }
}
