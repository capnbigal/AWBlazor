using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AWBlazorApp.Features.Engineering.Documents.Domain;

/// <summary>
/// Engineering document — drawing, specification, CAD model, procedure, etc. Stored as a
/// URL reference (the binary content lives outside the database). Optionally associated
/// with a <see cref="ProductId"/> for product-specific docs.
/// </summary>
[Table("EngineeringDocument", Schema = "eng")]
public class EngineeringDocument
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [MaxLength(32)] public string Code { get; set; } = string.Empty;

    [MaxLength(200)] public string Title { get; set; } = string.Empty;

    public EngineeringDocumentKind Kind { get; set; }

    /// <summary>FK → <c>Production.Product.ProductID</c>. Null for standard / cross-product docs.</summary>
    public int? ProductId { get; set; }

    public int RevisionNumber { get; set; } = 1;

    [MaxLength(1000)] public string? Url { get; set; }

    [MaxLength(2000)] public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime ModifiedDate { get; set; }
}

public enum EngineeringDocumentKind : byte
{
    Drawing = 1,
    Specification = 2,
    CadModel = 3,
    Procedure = 4,
    Other = 5,
}
