using AWBlazorApp.Features.AdventureWorks.Domain;
using System.ComponentModel.DataAnnotations;

namespace AWBlazorApp.Features.Mes.Domain;

public class DowntimeReasonAuditLog : AdventureWorksAuditLogBase
{
    public int DowntimeReasonId { get; set; }

    [MaxLength(32)] public string? Code { get; set; }
    [MaxLength(100)] public string? Name { get; set; }
    [MaxLength(500)] public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime SourceModifiedDate { get; set; }
}
