using AWBlazorApp.Shared.Domain;
using System.ComponentModel.DataAnnotations;

namespace AWBlazorApp.Features.Quality.Ncrs.Domain;

public class NonConformanceActionAuditLog : AdventureWorksAuditLogBase
{
    public int NonConformanceActionId { get; set; }

    public int NonConformanceId { get; set; }
    [MaxLength(500)] public string? Action { get; set; }
    [MaxLength(450)] public string? PerformedByUserId { get; set; }
    public DateTime PerformedAt { get; set; }
    [MaxLength(2000)] public string? Notes { get; set; }
    public DateTime SourceModifiedDate { get; set; }
}
