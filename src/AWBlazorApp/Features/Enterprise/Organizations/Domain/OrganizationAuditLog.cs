using AWBlazorApp.Shared.Domain;
using System.ComponentModel.DataAnnotations;

namespace AWBlazorApp.Features.Enterprise.Organizations.Domain;

public class OrganizationAuditLog : AdventureWorksAuditLogBase
{
    public int OrganizationId { get; set; }

    [MaxLength(32)]  public string? Code { get; set; }
    [MaxLength(200)] public string? Name { get; set; }
    public bool IsPrimary { get; set; }
    public int? ParentOrganizationId { get; set; }
    [MaxLength(128)] public string? ExternalRef { get; set; }
    public bool IsActive { get; set; }
    public DateTime SourceModifiedDate { get; set; }
}
