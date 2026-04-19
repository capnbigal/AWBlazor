namespace AWBlazorApp.Shared.UI.Components;

/// <summary>
/// Holds per-column distinct-value filter selections for a single grid. Each page constructs
/// one, hands it to every <c>ColumnFilterButton</c> in its columns, and passes it to
/// <c>ApplyColumnFilters</c> on the server side.
/// </summary>
public sealed class ColumnFilterState
{
    private readonly Dictionary<string, HashSet<object?>> _filters = new(StringComparer.Ordinal);

    /// <summary>Returns the mutable selection set for a column; creates an empty one on first access.</summary>
    public HashSet<object?> GetOrCreate(string column)
    {
        if (!_filters.TryGetValue(column, out var set))
            _filters[column] = set = new HashSet<object?>();
        return set;
    }

    public bool HasSelection(string column)
        => _filters.TryGetValue(column, out var set) && set.Count > 0;

    public void Clear(string column) => _filters.Remove(column);

    public void ClearAll() => _filters.Clear();

    /// <summary>Snapshot of active column → selected values. Safe to enumerate while filters mutate.</summary>
    public IReadOnlyDictionary<string, IReadOnlyCollection<object?>> Snapshot()
        => _filters
            .Where(kvp => kvp.Value.Count > 0)
            .ToDictionary(kvp => kvp.Key, kvp => (IReadOnlyCollection<object?>)kvp.Value.ToArray());
}
