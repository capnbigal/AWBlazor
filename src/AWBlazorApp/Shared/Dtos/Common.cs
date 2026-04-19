namespace AWBlazorApp.Shared.Dtos;

/// <summary>Standard response from POST/PATCH endpoints that mutate a single record.</summary>
public sealed record IdResponse(object Id);

/// <summary>Response from POST/PATCH endpoints whose entity has a string-typed primary key.</summary>
public sealed record StringIdResponse(string Id);

/// <summary>
/// Response from POST/PATCH endpoints whose entity has a composite primary key. The
/// <c>Key</c> dictionary carries each key column as a name/value pair so the caller can
/// reconstruct a /by-key URL or pass the key back into a follow-up call.
/// </summary>
public sealed record CompositeKeyResponse(IReadOnlyDictionary<string, object> Key);

/// <summary>Common pagination request — used by all list/query endpoints.</summary>
public sealed record PagedQuery
{
    public int Skip { get; init; }
    public int Take { get; init; } = 50;
}

/// <summary>
/// Standard list-response envelope. Mirrors the shape of the old AutoQuery `QueryResponse&lt;T&gt;`
/// so the rebuilt UI can bind to a familiar shape.
/// </summary>
public sealed record PagedResult<T>(IReadOnlyList<T> Results, int Total, int Skip, int Take);
