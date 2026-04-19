using AWBlazorApp.Shared.Domain;
using System.ComponentModel.DataAnnotations;

namespace AWBlazorApp.Features.Production.ProductSubcategories.Domain;

/// <summary>Audit log for <see cref="ProductSubcategory"/>. EF-managed table <c>dbo.ProductSubcategoryAuditLogs</c>.</summary>
public class ProductSubcategoryAuditLog : AdventureWorksAuditLogBase
{
    public int ProductSubcategoryId { get; set; }

    public int ProductCategoryId { get; set; }
    [MaxLength(50)] public string? Name { get; set; }
    public Guid RowGuid { get; set; }
    public DateTime SourceModifiedDate { get; set; }
}
