using AWBlazorApp.Shared.Domain;
using System.ComponentModel.DataAnnotations;

namespace AWBlazorApp.Features.Workforce.EmployeeQualifications.Domain;

public class EmployeeQualificationAuditLog : AdventureWorksAuditLogBase
{
    public int EmployeeQualificationId { get; set; }

    public int BusinessEntityId { get; set; }
    public int QualificationId { get; set; }
    public DateTime EarnedDate { get; set; }
    public DateTime? ExpiresOn { get; set; }
    [MaxLength(500)] public string? EvidenceUrl { get; set; }
    [MaxLength(450)] public string? VerifiedByUserId { get; set; }
    [MaxLength(500)] public string? Notes { get; set; }
    public DateTime SourceModifiedDate { get; set; }
}
