using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AWBlazorApp.Features.Enterprise.Domain;

/// <summary>
/// Physical machine/tool/fixture/vehicle. Anchors Phase F (Maintenance/CMMS) later. Sub-assemblies
/// reference a parent via <see cref="ParentAssetId"/>.
/// </summary>
[Table("Asset", Schema = "org")]
public class Asset
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public int OrganizationId { get; set; }

    /// <summary>Where the asset physically lives right now. Nullable for in-transit / decommissioned.</summary>
    public int? OrgUnitId { get; set; }

    [MaxLength(64)]
    public string AssetTag { get; set; } = string.Empty;

    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(128)]
    public string? Manufacturer { get; set; }

    [MaxLength(128)]
    public string? Model { get; set; }

    [MaxLength(128)]
    public string? SerialNumber { get; set; }

    public AssetType AssetType { get; set; }

    public DateTime? CommissionedAt { get; set; }

    public DateTime? DecommissionedAt { get; set; }

    public AssetStatus Status { get; set; } = AssetStatus.Active;

    public int? ParentAssetId { get; set; }

    public DateTime ModifiedDate { get; set; }
}

public enum AssetType : byte
{
    Machine = 1,
    Tool = 2,
    Fixture = 3,
    Vehicle = 4,
    Computer = 5,
    Instrument = 6,
    Other = 99,
}

public enum AssetStatus : byte
{
    Active = 1,
    Standby = 2,
    Maintenance = 3,
    Retired = 4,
}
