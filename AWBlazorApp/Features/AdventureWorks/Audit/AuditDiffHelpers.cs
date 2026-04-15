using System.Text;

namespace AWBlazorApp.Features.AdventureWorks.Audit;

/// <summary>
/// Shared formatting helpers used by the per-table AdventureWorks audit services to build
/// human-readable diff summaries (e.g. <c>"Name: Foo → Bar; IsActive: True → False"</c>).
/// </summary>
public static class AuditDiffHelpers
{
    /// <summary>
    /// Append a "<paramref name="name"/>: before → after" clause to <paramref name="sb"/>
    /// iff <paramref name="before"/> and <paramref name="after"/> differ by the default
    /// equality comparer for <typeparamref name="T"/>.
    /// </summary>
    public static void AppendIfChanged<T>(StringBuilder sb, string name, T? before, T? after)
    {
        if (EqualityComparer<T?>.Default.Equals(before, after)) return;
        AppendSeparator(sb);
        sb.Append(name).Append(": ").Append(Format(before)).Append(" → ").Append(Format(after));
    }

    /// <summary>Appends "; " if the builder is already non-empty.</summary>
    public static void AppendSeparator(StringBuilder sb)
    {
        if (sb.Length > 0) sb.Append("; ");
    }

    /// <summary>Stringifies a value for diff output, substituting "(empty)" for null / empty strings.</summary>
    public static string Format(object? v)
    {
        if (v is null) return "(empty)";
        if (v is string s) return string.IsNullOrEmpty(s) ? "(empty)" : s;
        return v.ToString() ?? "(empty)";
    }

    /// <summary>Truncate <paramref name="value"/> to at most <paramref name="maxLength"/> characters.</summary>
    public static string? Truncate(string? value, int maxLength)
    {
        if (value is null) return null;
        return value.Length <= maxLength ? value : value[..(maxLength - 1)] + "…";
    }
}
