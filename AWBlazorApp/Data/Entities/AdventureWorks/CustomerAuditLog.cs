using System.ComponentModel.DataAnnotations;

namespace AWBlazorApp.Data.Entities.AdventureWorks;

/// <summary>Audit log for <see cref="Customer"/>. EF-managed table <c>dbo.CustomerAuditLogs</c>.</summary>
public class CustomerAuditLog : AdventureWorksAuditLogBase
{
    public int CustomerId { get; set; }

    public int? PersonId { get; set; }
    public int? StoreId { get; set; }
    public int? TerritoryId { get; set; }
    [MaxLength(10)] public string? AccountNumber { get; set; }
    public Guid RowGuid { get; set; }
    public DateTime SourceModifiedDate { get; set; }
}
