using AWBlazorApp.Features.AdventureWorks.Domain;
using System.ComponentModel.DataAnnotations;

namespace AWBlazorApp.Features.Enterprise.Domain;

public class CostCenterAuditLog : AdventureWorksAuditLogBase
{
    public int CostCenterId { get; set; }

    public int OrganizationId { get; set; }
    [MaxLength(32)]  public string? Code { get; set; }
    [MaxLength(200)] public string? Name { get; set; }
    public int? OwnerBusinessEntityId { get; set; }
    public bool IsActive { get; set; }
    public DateTime SourceModifiedDate { get; set; }
}
