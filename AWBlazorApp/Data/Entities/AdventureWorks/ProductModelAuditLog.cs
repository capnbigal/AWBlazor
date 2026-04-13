using System.ComponentModel.DataAnnotations;

namespace AWBlazorApp.Data.Entities.AdventureWorks;

/// <summary>Audit log for <see cref="ProductModel"/>. EF-managed table <c>dbo.ProductModelAuditLogs</c>.</summary>
public class ProductModelAuditLog : AdventureWorksAuditLogBase
{
    public int ProductModelId { get; set; }

    [MaxLength(50)] public string? Name { get; set; }
    public Guid RowGuid { get; set; }
    public DateTime SourceModifiedDate { get; set; }
}
