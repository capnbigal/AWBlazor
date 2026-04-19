using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AWBlazorApp.Features.Engineering.Routings.Domain;

/// <summary>Ordered operation within a <see cref="ManufacturingRouting"/>.</summary>
[Table("RoutingStep", Schema = "eng")]
public class RoutingStep
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public int ManufacturingRoutingId { get; set; }

    public int SequenceNumber { get; set; }

    [MaxLength(200)] public string OperationName { get; set; } = string.Empty;

    /// <summary>FK → enterprise <c>Station</c>. Nullable for operations done off-station (e.g., inspection at QA bench).</summary>
    public int? StationId { get; set; }

    public decimal StandardMinutes { get; set; }

    [MaxLength(2000)] public string? Instructions { get; set; }

    public DateTime ModifiedDate { get; set; }
}
