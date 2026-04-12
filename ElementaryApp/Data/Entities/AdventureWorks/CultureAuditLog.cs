using System.ComponentModel.DataAnnotations;

namespace ElementaryApp.Data.Entities.AdventureWorks;

/// <summary>Audit log for <see cref="Culture"/>. EF-managed table <c>dbo.CultureAuditLogs</c>.</summary>
public class CultureAuditLog : AdventureWorksAuditLogBase
{
    /// <summary>String PK of the affected row (matches <see cref="Culture.CultureId"/>).</summary>
    [MaxLength(6)]
    public string CultureId { get; set; } = string.Empty;

    [MaxLength(50)] public string? Name { get; set; }
    public DateTime SourceModifiedDate { get; set; }
}
