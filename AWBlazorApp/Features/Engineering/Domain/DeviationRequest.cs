using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AWBlazorApp.Features.Engineering.Domain;

/// <summary>
/// A temporary, bounded departure from a product's approved specification. Unlike an ECO
/// (which updates the spec), a deviation lets production continue with out-of-spec material
/// for a limited scope. Workflow: Pending → Approved / Rejected / Cancelled. Time-bounded
/// via <see cref="ValidFrom"/> / <see cref="ValidTo"/>; quantity-bounded via
/// <see cref="AuthorizedQuantity"/>.
/// </summary>
[Table("DeviationRequest", Schema = "eng")]
public class DeviationRequest
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [MaxLength(32)] public string Code { get; set; } = string.Empty;

    /// <summary>FK → <c>Production.Product.ProductID</c>.</summary>
    public int ProductId { get; set; }

    [MaxLength(2000)] public string Reason { get; set; } = string.Empty;

    [MaxLength(2000)] public string? ProposedDisposition { get; set; }

    [Column(TypeName = "decimal(18,2)")] public decimal? AuthorizedQuantity { get; set; }
    [MaxLength(3)] public string? UnitMeasureCode { get; set; }

    public DateOnly? ValidFrom { get; set; }
    public DateOnly? ValidTo { get; set; }

    public DeviationStatus Status { get; set; } = DeviationStatus.Pending;

    [MaxLength(450)] public string? RaisedByUserId { get; set; }
    public DateTime RaisedAt { get; set; }

    [MaxLength(450)] public string? DecidedByUserId { get; set; }
    public DateTime? DecidedAt { get; set; }
    [MaxLength(2000)] public string? DecisionNotes { get; set; }

    public DateTime ModifiedDate { get; set; }
}

public enum DeviationStatus : byte
{
    Pending = 1,
    Approved = 2,
    Rejected = 3,
    Cancelled = 4,
    Expired = 5,
}
