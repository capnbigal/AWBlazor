using AWBlazorApp.Shared.Domain;
using System.ComponentModel.DataAnnotations;

namespace AWBlazorApp.Features.Quality.Capa.Domain;

public class CapaCaseAuditLog : AdventureWorksAuditLogBase
{
    public int CapaCaseId { get; set; }

    [MaxLength(32)] public string? CaseNumber { get; set; }
    [MaxLength(200)] public string? Title { get; set; }
    public CapaStatus Status { get; set; }
    [MaxLength(2000)] public string? RootCause { get; set; }
    [MaxLength(2000)] public string? CorrectiveAction { get; set; }
    [MaxLength(2000)] public string? PreventiveAction { get; set; }
    [MaxLength(2000)] public string? VerificationNotes { get; set; }
    public int? OwnerBusinessEntityId { get; set; }
    public DateTime OpenedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public DateTime SourceModifiedDate { get; set; }
}
