using AWBlazorApp.Shared.Domain;
using System.ComponentModel.DataAnnotations;

namespace AWBlazorApp.Features.Workforce.Domain;

public class AnnouncementAuditLog : AdventureWorksAuditLogBase
{
    public int AnnouncementId { get; set; }

    [MaxLength(200)] public string? Title { get; set; }
    public AnnouncementSeverity Severity { get; set; }
    public int? OrganizationId { get; set; }
    public int? OrgUnitId { get; set; }
    public DateTime PublishedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    [MaxLength(450)] public string? AuthoredByUserId { get; set; }
    public bool IsActive { get; set; }
    public DateTime SourceModifiedDate { get; set; }
}
