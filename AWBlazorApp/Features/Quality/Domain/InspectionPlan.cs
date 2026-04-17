using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AWBlazorApp.Features.Quality.Domain;

/// <summary>
/// Reusable inspection-plan template. Scoped via any combination of <see cref="ProductId"/>,
/// <see cref="WorkOrderRoutingId"/>, and <see cref="VendorBusinessEntityId"/> — at least one is
/// required (validator enforces). The <see cref="Scope"/> enum narrows when the plan applies
/// (inbound receipts / in-process WOs / outbound shipments / vendor-specific first-piece).
/// <see cref="AutoTriggerOnReceipt"/>, <c>…OnShipment</c>, and <c>…OnProductionRun</c> flags
/// flip the corresponding <c>IPostingTriggerHook</c> path into creating a Pending inspection
/// every time an upstream posting matches the plan's scope.
/// </summary>
[Table("InspectionPlan", Schema = "qa")]
public class InspectionPlan
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [MaxLength(32)] public string PlanCode { get; set; } = string.Empty;
    [MaxLength(200)] public string Name { get; set; } = string.Empty;
    [MaxLength(2000)] public string? Description { get; set; }

    public InspectionScope Scope { get; set; } = InspectionScope.Inbound;

    public int? ProductId { get; set; }
    public int? WorkOrderRoutingId { get; set; }
    public int? VendorBusinessEntityId { get; set; }

    [MaxLength(200)] public string? SamplingRule { get; set; }

    public bool AutoTriggerOnReceipt { get; set; }
    public bool AutoTriggerOnShipment { get; set; }
    public bool AutoTriggerOnProductionRun { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime ModifiedDate { get; set; }
}

public enum InspectionScope : byte
{
    Inbound = 1,
    InProcess = 2,
    Outbound = 3,
    Vendor = 4,
}
