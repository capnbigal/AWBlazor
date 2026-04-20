using AWBlazorApp.Shared.Domain;
using System.ComponentModel.DataAnnotations;

namespace AWBlazorApp.Features.Quality.Ncrs.Domain;

public class NonConformanceActionAuditLog : AdventureWorksAuditLogBase
{
    public int NonConformanceActionId { get; set; }

    public int NonConformanceId { get; set; }
    // Hides AdventureWorksAuditLogBase.Action by design — for action audit logs the "Action"
    // column carries the user-described action description (free-text), not the create/update/
    // delete verb tracked by the base type.
    [MaxLength(500)] public new string? Action { get; set; }
    [MaxLength(450)] public string? PerformedByUserId { get; set; }
    public DateTime PerformedAt { get; set; }
    [MaxLength(2000)] public string? Notes { get; set; }
    public DateTime SourceModifiedDate { get; set; }
}
