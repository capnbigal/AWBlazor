using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AWBlazorApp.Features.Workforce.Domain;

/// <summary>
/// Broader broadcast — org-wide when both <see cref="OrganizationId"/> and
/// <see cref="OrgUnitId"/> are null, scoped to an org when only OrganizationId is set, or
/// to a specific OrgUnit when both are set. Drives an "announcements" feed in the workforce
/// summary.
/// </summary>
[Table("Announcement", Schema = "wf")]
public class Announcement
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [MaxLength(200)] public string Title { get; set; } = string.Empty;

    [Column(TypeName = "nvarchar(max)")]
    public string Body { get; set; } = string.Empty;

    public AnnouncementSeverity Severity { get; set; } = AnnouncementSeverity.Info;

    public int? OrganizationId { get; set; }
    public int? OrgUnitId { get; set; }

    public DateTime PublishedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }

    [MaxLength(450)] public string? AuthoredByUserId { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime ModifiedDate { get; set; }
}

public enum AnnouncementSeverity : byte
{
    Info = 1,
    Important = 2,
    Critical = 3,
}
