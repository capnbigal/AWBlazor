using AWBlazorApp.Features.AdventureWorks.Domain;
using System.ComponentModel.DataAnnotations;

namespace AWBlazorApp.Features.Enterprise.Domain;

public class OrgUnitAuditLog : AdventureWorksAuditLogBase
{
    public int OrgUnitId { get; set; }

    public int OrganizationId { get; set; }
    public int? ParentOrgUnitId { get; set; }
    public OrgUnitKind Kind { get; set; }
    [MaxLength(32)]   public string? Code { get; set; }
    [MaxLength(200)]  public string? Name { get; set; }
    [MaxLength(1024)] public string? Path { get; set; }
    public byte Depth { get; set; }
    public int? CostCenterId { get; set; }
    public int? ManagerBusinessEntityId { get; set; }
    public bool IsActive { get; set; }
    public DateTime SourceModifiedDate { get; set; }
}
