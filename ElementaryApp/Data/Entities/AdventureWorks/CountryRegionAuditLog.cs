using System.ComponentModel.DataAnnotations;

namespace ElementaryApp.Data.Entities.AdventureWorks;

/// <summary>Audit log for <see cref="CountryRegion"/>. EF-managed table <c>dbo.CountryRegionAuditLogs</c>.</summary>
public class CountryRegionAuditLog : AdventureWorksAuditLogBase
{
    /// <summary>String PK of the affected row (matches <see cref="CountryRegion.CountryRegionCode"/>).</summary>
    [MaxLength(3)]
    public string CountryRegionCode { get; set; } = string.Empty;

    [MaxLength(50)] public string? Name { get; set; }
    public DateTime SourceModifiedDate { get; set; }
}
