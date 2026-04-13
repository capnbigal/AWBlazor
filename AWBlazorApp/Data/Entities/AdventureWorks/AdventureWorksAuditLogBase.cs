using System.ComponentModel.DataAnnotations;

namespace AWBlazorApp.Data.Entities.AdventureWorks;

/// <summary>
/// Common audit fields for every AdventureWorks reference-data audit log entity. Concrete
/// subclasses add a single <c>&lt;Table&gt;Id</c> key column plus snapshot columns mirroring the
/// source table. The base class itself is abstract and never gets its own DbSet, so EF doesn't
/// try to map it to a table — each concrete derivative lives in its own table (TPC by virtue
/// of only the derivatives being registered).
/// </summary>
public abstract class AdventureWorksAuditLogBase
{
    public int Id { get; set; }

    /// <summary>One of <c>Created</c>, <c>Updated</c>, <c>Deleted</c>.</summary>
    [MaxLength(16)]
    public string Action { get; set; } = string.Empty;

    /// <summary>Identity name of the user who made the change (may be null for background jobs / API keys).</summary>
    [MaxLength(256)]
    public string? ChangedBy { get; set; }

    public DateTime ChangedDate { get; set; }

    /// <summary>
    /// Human-readable diff summary. For <c>Updated</c> rows, something like
    /// "Name: Foo → Bar". For <c>Created</c>/<c>Deleted</c> rows, a short tag.
    /// </summary>
    [MaxLength(2048)]
    public string? ChangeSummary { get; set; }
}
