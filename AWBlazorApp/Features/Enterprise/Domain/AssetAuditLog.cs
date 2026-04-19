using AWBlazorApp.Shared.Domain;
using System.ComponentModel.DataAnnotations;

namespace AWBlazorApp.Features.Enterprise.Domain;

public class AssetAuditLog : AdventureWorksAuditLogBase
{
    public int AssetId { get; set; }

    public int OrganizationId { get; set; }
    public int? OrgUnitId { get; set; }
    [MaxLength(64)]  public string? AssetTag { get; set; }
    [MaxLength(200)] public string? Name { get; set; }
    [MaxLength(128)] public string? Manufacturer { get; set; }
    [MaxLength(128)] public string? Model { get; set; }
    [MaxLength(128)] public string? SerialNumber { get; set; }
    public AssetType AssetType { get; set; }
    public DateTime? CommissionedAt { get; set; }
    public DateTime? DecommissionedAt { get; set; }
    public AssetStatus Status { get; set; }
    public int? ParentAssetId { get; set; }
    public DateTime SourceModifiedDate { get; set; }
}
