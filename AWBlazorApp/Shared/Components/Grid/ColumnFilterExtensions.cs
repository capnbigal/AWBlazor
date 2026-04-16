using System.Collections;
using System.Linq.Expressions;
using System.Reflection;

namespace AWBlazorApp.Shared.Components.Grid;

/// <summary>
/// Applies a <see cref="ColumnFilterState"/> to an <see cref="IQueryable{T}"/> by building
/// <c>.Where(x =&gt; selectedValues.Contains(x.Prop))</c> predicates per selected column.
/// </summary>
public static class ColumnFilterExtensions
{
    public static IQueryable<T> ApplyColumnFilters<T>(this IQueryable<T> query, ColumnFilterState state)
    {
        foreach (var (column, values) in state.Snapshot())
        {
            var property = typeof(T).GetProperty(column, BindingFlags.Public | BindingFlags.Instance);
            if (property is null) continue;

            query = query.Where(BuildContainsPredicate<T>(property, values));
        }
        return query;
    }

    /// <summary>
    /// Builds <c>x =&gt; typedValues.Contains(x.Prop)</c> where <c>typedValues</c> is a strongly-typed
    /// list matching the property's type. Required because EF can't translate a heterogeneous
    /// <c>object</c>-typed <c>HashSet&lt;object?&gt;.Contains</c> into SQL.
    /// </summary>
    private static Expression<Func<T, bool>> BuildContainsPredicate<T>(
        PropertyInfo property, IReadOnlyCollection<object?> rawValues)
    {
        var propType = property.PropertyType;
        var listType = typeof(List<>).MakeGenericType(propType);
        var typedList = (IList)Activator.CreateInstance(listType)!;
        foreach (var v in rawValues)
        {
            typedList.Add(v is null ? null : ConvertToPropertyType(v, propType));
        }

        var parameter = Expression.Parameter(typeof(T), "x");
        var propertyAccess = Expression.Property(parameter, property);

        var containsMethod = typeof(Enumerable)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .First(m => m.Name == nameof(Enumerable.Contains) && m.GetParameters().Length == 2)
            .MakeGenericMethod(propType);

        var body = Expression.Call(containsMethod, Expression.Constant(typedList, listType), propertyAccess);
        return Expression.Lambda<Func<T, bool>>(body, parameter);
    }

    private static object ConvertToPropertyType(object value, Type target)
    {
        var underlying = Nullable.GetUnderlyingType(target) ?? target;
        if (underlying.IsInstanceOfType(value)) return value;
        if (underlying.IsEnum) return Enum.Parse(underlying, value.ToString()!, ignoreCase: true);
        return Convert.ChangeType(value, underlying);
    }
}
