using AWBlazorApp.Features.AdventureWorks.Domain;
using System.ComponentModel.DataAnnotations;

namespace AWBlazorApp.Features.Quality.Domain;

public class InspectionPlanAuditLog : AdventureWorksAuditLogBase
{
    public int InspectionPlanId { get; set; }

    [MaxLength(32)] public string? PlanCode { get; set; }
    [MaxLength(200)] public string? Name { get; set; }
    [MaxLength(2000)] public string? Description { get; set; }
    public InspectionScope Scope { get; set; }
    public int? ProductId { get; set; }
    public int? WorkOrderRoutingId { get; set; }
    public int? VendorBusinessEntityId { get; set; }
    [MaxLength(200)] public string? SamplingRule { get; set; }
    public bool AutoTriggerOnReceipt { get; set; }
    public bool AutoTriggerOnShipment { get; set; }
    public bool AutoTriggerOnProductionRun { get; set; }
    public bool IsActive { get; set; }
    public DateTime SourceModifiedDate { get; set; }
}
