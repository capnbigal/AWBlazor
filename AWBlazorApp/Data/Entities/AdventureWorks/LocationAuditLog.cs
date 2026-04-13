using System.ComponentModel.DataAnnotations;

namespace AWBlazorApp.Data.Entities.AdventureWorks;

/// <summary>Audit log for <see cref="Location"/>. EF-managed table <c>dbo.LocationAuditLogs</c>.</summary>
public class LocationAuditLog : AdventureWorksAuditLogBase
{
    public short LocationId { get; set; }

    [MaxLength(50)] public string? Name { get; set; }
    public decimal CostRate { get; set; }
    public decimal Availability { get; set; }
    public DateTime SourceModifiedDate { get; set; }
}
