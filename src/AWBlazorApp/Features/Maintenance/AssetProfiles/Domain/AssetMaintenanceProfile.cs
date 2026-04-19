using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AWBlazorApp.Features.Maintenance.AssetProfiles.Domain;

/// <summary>
/// 1:1 maintenance sidecar on <c>org.Asset</c>. Keeps the enterprise Asset schema stable while
/// letting the maintenance module attach criticality, ownership, target MTBF, and a running
/// "next PM due" hint. Unique on <see cref="AssetId"/>.
/// </summary>
[Table("AssetMaintenanceProfile", Schema = "maint")]
public class AssetMaintenanceProfile
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public int AssetId { get; set; }

    public AssetCriticality Criticality { get; set; } = AssetCriticality.Medium;

    /// <summary>FK → <c>HumanResources.Employee.BusinessEntityID</c>. Primary owner / responsible tech.</summary>
    public int? OwnerBusinessEntityId { get; set; }

    /// <summary>Mean time between failures target, in hours. Null = no target tracked yet.</summary>
    public int? TargetMtbfHours { get; set; }

    /// <summary>Computed convenience — set by PmScheduleService when a PM is due. Null = none due.</summary>
    public DateTime? NextPmDueAt { get; set; }

    [MaxLength(2000)] public string? Notes { get; set; }

    public DateTime ModifiedDate { get; set; }
}

public enum AssetCriticality : byte
{
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4,
}
