using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AWBlazorApp.Features.Engineering.Domain;

/// <summary>
/// Engineering Change Order. Workflow: Draft → UnderReview → Approved or Rejected. On
/// Approved, <see cref="Services.IEcoService.ApproveAsync"/> auto-activates any affected
/// BOM / Routing revisions referenced in <see cref="EcoAffectedItem"/> rows and deactivates
/// the prior active revision for the same product.
/// </summary>
[Table("EngineeringChangeOrder", Schema = "eng")]
public class EngineeringChangeOrder
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [MaxLength(32)] public string Code { get; set; } = string.Empty;

    [MaxLength(200)] public string Title { get; set; } = string.Empty;

    [MaxLength(4000)] public string? Description { get; set; }

    public EcoStatus Status { get; set; } = EcoStatus.Draft;

    [MaxLength(450)] public string? RaisedByUserId { get; set; }
    public DateTime RaisedAt { get; set; }

    public DateTime? SubmittedAt { get; set; }

    public DateTime? DecidedAt { get; set; }
    [MaxLength(450)] public string? DecidedByUserId { get; set; }
    [MaxLength(2000)] public string? DecisionNotes { get; set; }

    public DateTime ModifiedDate { get; set; }
}

public enum EcoStatus : byte
{
    Draft = 1,
    UnderReview = 2,
    Approved = 3,
    Rejected = 4,
    Cancelled = 5,
}
