using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AWBlazorApp.Features.Engineering.Domain;

/// <summary>
/// An item affected by an ECO. When the ECO is approved:
///   - <see cref="EcoAffectedKind.Bom"/> / <see cref="EcoAffectedKind.Routing"/>: the row
///     referenced by <see cref="TargetId"/> is marked active and any other active rows for
///     the same ProductId are cleared.
///   - <see cref="EcoAffectedKind.Product"/> / <see cref="EcoAffectedKind.Document"/>: no
///     automatic activation — these are informational references.
/// </summary>
[Table("EcoAffectedItem", Schema = "eng")]
public class EcoAffectedItem
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public int EngineeringChangeOrderId { get; set; }

    public EcoAffectedKind AffectedKind { get; set; }

    /// <summary>PK of the referenced entity — interpretation depends on <see cref="AffectedKind"/>.</summary>
    public int TargetId { get; set; }

    [MaxLength(500)] public string? Notes { get; set; }

    public DateTime ModifiedDate { get; set; }
}

public enum EcoAffectedKind : byte
{
    Product = 1,
    Bom = 2,
    Routing = 3,
    Document = 4,
}
