using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AWBlazorApp.Features.Quality.Domain;

/// <summary>
/// Non-conformance report (NCR). Opened when an inspection fails or someone flags a problem
/// manually. The disposition determines whether and how we touch inventory:
/// Scrap posts a SCRAP transaction removing the qty from stock; Quarantine posts a paired
/// MOVE (same location, Available → Quarantine balance status); Rework, UseAsIs, and
/// ReturnToSupplier don't post anything automatically — those require human process.
/// </summary>
[Table("NonConformance", Schema = "qa")]
public class NonConformance
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [MaxLength(32)] public string NcrNumber { get; set; } = string.Empty;

    /// <summary>Source inspection (nullable — NCRs can be opened manually without a failed inspection).</summary>
    public int? InspectionId { get; set; }

    public int InventoryItemId { get; set; }
    public int? LotId { get; set; }
    public int? LocationId { get; set; }

    [Column(TypeName = "decimal(18,4)")]
    public decimal Quantity { get; set; }

    [MaxLength(3)]
    public string UnitMeasureCode { get; set; } = "EA";

    [MaxLength(2000)] public string Description { get; set; } = string.Empty;

    public NonConformanceStatus Status { get; set; } = NonConformanceStatus.Open;

    public NonConformanceDisposition? Disposition { get; set; }

    [MaxLength(450)] public string? DispositionedByUserId { get; set; }
    public DateTime? DispositionedAt { get; set; }

    [MaxLength(2000)] public string? DispositionNotes { get; set; }

    /// <summary>Set when <see cref="Disposition"/> is Scrap or Quarantine and the inventory
    /// transaction has been posted.</summary>
    public long? PostedTransactionId { get; set; }

    public DateTime ModifiedDate { get; set; }
}

public enum NonConformanceStatus : byte
{
    Open = 1,
    Investigating = 2,
    Dispositioned = 3,
    Closed = 4,
}

public enum NonConformanceDisposition : byte
{
    Rework = 1,
    Scrap = 2,
    UseAsIs = 3,
    ReturnToSupplier = 4,
    Quarantine = 5,
}
