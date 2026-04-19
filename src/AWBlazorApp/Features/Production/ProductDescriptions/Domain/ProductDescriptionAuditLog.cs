using AWBlazorApp.Shared.Domain;
using System.ComponentModel.DataAnnotations;

namespace AWBlazorApp.Features.Production.ProductDescriptions.Domain;

/// <summary>Audit log for <see cref="ProductDescription"/>. EF-managed table <c>dbo.ProductDescriptionAuditLogs</c>.</summary>
public class ProductDescriptionAuditLog : AdventureWorksAuditLogBase
{
    public int ProductDescriptionId { get; set; }

    [MaxLength(400)] public string? Description { get; set; }
    public Guid RowGuid { get; set; }
    public DateTime SourceModifiedDate { get; set; }
}
