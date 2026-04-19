using AWBlazorApp.Shared.Domain;
using System.ComponentModel.DataAnnotations;

namespace AWBlazorApp.Features.Production.Domain;

/// <summary>Audit log for <see cref="ProductModelProductDescriptionCulture"/>. EF-managed table <c>dbo.ProductModelProductDescriptionCultureAuditLogs</c>. A pure junction table — audit rows only carry the composite key plus timestamp.</summary>
public class ProductModelProductDescriptionCultureAuditLog : AdventureWorksAuditLogBase
{
    public int ProductModelId { get; set; }
    public int ProductDescriptionId { get; set; }
    [MaxLength(6)] public string CultureId { get; set; } = string.Empty;

    public DateTime SourceModifiedDate { get; set; }
}
