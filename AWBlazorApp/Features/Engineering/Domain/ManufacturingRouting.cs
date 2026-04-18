using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AWBlazorApp.Features.Engineering.Domain;

/// <summary>
/// A reusable routing template — the ordered sequence of stations and operations used to
/// produce a <c>Production.Product</c>. Distinct from AdventureWorks'
/// <c>Production.WorkOrderRouting</c>, which is per-order. The ECO workflow activates a new
/// revision by setting <see cref="IsActive"/> on the new row and clearing it on prior rows
/// for the same <see cref="ProductId"/>.
/// </summary>
[Table("ManufacturingRouting", Schema = "eng")]
public class ManufacturingRouting
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [MaxLength(32)] public string Code { get; set; } = string.Empty;
    [MaxLength(200)] public string Name { get; set; } = string.Empty;
    [MaxLength(2000)] public string? Description { get; set; }

    /// <summary>FK → <c>Production.Product.ProductID</c>.</summary>
    public int ProductId { get; set; }

    /// <summary>Monotonically-increasing revision number for this product's routing.</summary>
    public int RevisionNumber { get; set; } = 1;

    public bool IsActive { get; set; } = true;

    public DateTime ModifiedDate { get; set; }
}
