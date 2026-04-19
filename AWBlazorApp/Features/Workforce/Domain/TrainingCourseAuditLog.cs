using AWBlazorApp.Shared.Domain;
using System.ComponentModel.DataAnnotations;

namespace AWBlazorApp.Features.Workforce.Domain;

public class TrainingCourseAuditLog : AdventureWorksAuditLogBase
{
    public int TrainingCourseId { get; set; }

    [MaxLength(32)] public string? Code { get; set; }
    [MaxLength(200)] public string? Name { get; set; }
    [MaxLength(2000)] public string? Description { get; set; }
    public int? DurationMinutes { get; set; }
    public int? RecurrenceMonths { get; set; }
    public bool IsActive { get; set; }
    public DateTime SourceModifiedDate { get; set; }
}
