using AWBlazorApp.Features.AdventureWorks.Domain;
using System.ComponentModel.DataAnnotations;

namespace AWBlazorApp.Features.Maintenance.Domain;

public class AssetMaintenanceProfileAuditLog : AdventureWorksAuditLogBase
{
    public int AssetMaintenanceProfileId { get; set; }

    public int AssetId { get; set; }
    public AssetCriticality Criticality { get; set; }
    public int? OwnerBusinessEntityId { get; set; }
    public int? TargetMtbfHours { get; set; }
    public DateTime? NextPmDueAt { get; set; }
    [MaxLength(2000)] public string? Notes { get; set; }
    public DateTime SourceModifiedDate { get; set; }
}
