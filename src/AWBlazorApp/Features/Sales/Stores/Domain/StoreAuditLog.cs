using AWBlazorApp.Shared.Domain;
using System.ComponentModel.DataAnnotations;

namespace AWBlazorApp.Features.Sales.Stores.Domain;

/// <summary>Audit log for <see cref="Store"/>. EF-managed table <c>dbo.StoreAuditLogs</c>.</summary>
public class StoreAuditLog : AdventureWorksAuditLogBase
{
    public int StoreId { get; set; }

    [MaxLength(50)] public string? Name { get; set; }
    public int? SalesPersonId { get; set; }
    public Guid RowGuid { get; set; }
    public DateTime SourceModifiedDate { get; set; }
}
