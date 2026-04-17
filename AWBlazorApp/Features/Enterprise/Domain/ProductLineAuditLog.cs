using AWBlazorApp.Features.AdventureWorks.Domain;
using System.ComponentModel.DataAnnotations;

namespace AWBlazorApp.Features.Enterprise.Domain;

public class ProductLineAuditLog : AdventureWorksAuditLogBase
{
    public int ProductLineId { get; set; }

    public int OrganizationId { get; set; }
    [MaxLength(32)]   public string? Code { get; set; }
    [MaxLength(200)]  public string? Name { get; set; }
    [MaxLength(2000)] public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime SourceModifiedDate { get; set; }
}
