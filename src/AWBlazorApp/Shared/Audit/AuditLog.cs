using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AWBlazorApp.Shared.Audit;

/// <summary>
/// Consolidated audit-log table that replaces the 117 per-entity <c>*AuditLog</c> tables
/// scattered across the codebase. Rows are written automatically by
/// <see cref="Infrastructure.Persistence.AuditingInterceptor"/> on every
/// <c>SaveChangesAsync</c>.
/// </summary>
/// <remarks>
/// <para>
/// Primary keys are stringified to support int / long / Guid / composite keys uniformly.
/// For composite keys, values are joined with <c>"|"</c>.
/// </para>
/// <para>
/// <see cref="ChangesJson"/> is a JSON document with shape:
/// <list type="bullet">
///   <item><c>Created</c>: <c>{ "FieldName": postValue, ... }</c></item>
///   <item><c>Updated</c>: <c>{ "FieldName": { "before": preValue, "after": postValue }, ... }</c> — only changed fields appear</item>
///   <item><c>Deleted</c>: <c>{ "FieldName": preValue, ... }</c></item>
/// </list>
/// </para>
/// </remarks>
[Table("AuditLog", Schema = "audit")]
public class AuditLog
{
    [Key]
    public long Id { get; set; }

    /// <summary>CLR type name of the audited entity (e.g. <c>"CostCenter"</c>).</summary>
    [MaxLength(128)]
    public string EntityType { get; set; } = "";

    /// <summary>Stringified primary key of the audited row (e.g. <c>"42"</c>, <c>"guid-value"</c>, or <c>"5|Primary"</c> for composite keys).</summary>
    [MaxLength(128)]
    public string EntityId { get; set; } = "";

    /// <summary>One of <c>Created</c>, <c>Updated</c>, <c>Deleted</c>.</summary>
    [MaxLength(16)]
    public string Action { get; set; } = "";

    /// <summary>Identity name of the user who made the change (null for background jobs / seed / API-key-without-user).</summary>
    [MaxLength(450)]
    public string? ChangedBy { get; set; }

    public DateTime ChangedDate { get; set; }

    /// <summary>JSON diff payload — see <see cref="AuditLog"/> remarks for the shape.</summary>
    public string? ChangesJson { get; set; }

    /// <summary>Human-readable summary (e.g. <c>"Name: Foo → Bar; IsActive: True → False"</c>), cached for list views.</summary>
    [MaxLength(2048)]
    public string? Summary { get; set; }
}
