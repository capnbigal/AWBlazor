using System.ComponentModel.DataAnnotations;

namespace AWBlazorApp.Features.AdventureWorks.Domain;

/// <summary>Audit log for <see cref="UnitMeasure"/>. EF-managed table <c>dbo.UnitMeasureAuditLogs</c>.</summary>
public class UnitMeasureAuditLog : AdventureWorksAuditLogBase
{
    /// <summary>String PK of the affected row (matches <see cref="UnitMeasure.UnitMeasureCode"/>).</summary>
    [MaxLength(3)]
    public string UnitMeasureCode { get; set; } = string.Empty;

    [MaxLength(50)] public string? Name { get; set; }
    public DateTime SourceModifiedDate { get; set; }
}
