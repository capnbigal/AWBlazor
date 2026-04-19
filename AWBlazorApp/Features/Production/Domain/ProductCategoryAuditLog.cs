using AWBlazorApp.Shared.Domain;
using System.ComponentModel.DataAnnotations;

namespace AWBlazorApp.Features.Production.Domain;

/// <summary>Audit log for <see cref="ProductCategory"/>. EF-managed table <c>dbo.ProductCategoryAuditLogs</c>.</summary>
public class ProductCategoryAuditLog : AdventureWorksAuditLogBase
{
    public int ProductCategoryId { get; set; }

    [MaxLength(50)] public string? Name { get; set; }
    public Guid RowGuid { get; set; }
    public DateTime SourceModifiedDate { get; set; }
}
