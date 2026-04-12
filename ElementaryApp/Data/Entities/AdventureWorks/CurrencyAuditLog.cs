using System.ComponentModel.DataAnnotations;

namespace ElementaryApp.Data.Entities.AdventureWorks;

/// <summary>Audit log for <see cref="Currency"/>. EF-managed table <c>dbo.CurrencyAuditLogs</c>.</summary>
public class CurrencyAuditLog : AdventureWorksAuditLogBase
{
    /// <summary>String PK of the affected row (matches <see cref="Currency.CurrencyCode"/>).</summary>
    [MaxLength(3)]
    public string CurrencyCode { get; set; } = string.Empty;

    [MaxLength(50)] public string? Name { get; set; }
    public DateTime SourceModifiedDate { get; set; }
}
