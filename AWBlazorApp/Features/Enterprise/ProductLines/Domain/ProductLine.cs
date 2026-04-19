using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AWBlazorApp.Features.Enterprise.ProductLines.Domain;

/// <summary>
/// Logical grouping that a <c>Production.Product</c> can belong to — a management-level view
/// above ProductCategory/ProductSubcategory. Linkage to Product is loose (optional FK on Product
/// added later, or kept as a view) so this table can be managed independently.
/// </summary>
[Table("ProductLine", Schema = "org")]
public class ProductLine
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public int OrganizationId { get; set; }

    [MaxLength(32)]
    public string Code { get; set; } = string.Empty;

    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime ModifiedDate { get; set; }
}
