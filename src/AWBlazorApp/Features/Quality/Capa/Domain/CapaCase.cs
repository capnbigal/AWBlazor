using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AWBlazorApp.Features.Quality.Capa.Domain;

/// <summary>
/// Corrective + Preventive Action case. Wraps one or more <see cref="NonConformance"/> records
/// (many-to-many via <see cref="CapaCaseNonConformance"/>) so recurring issues can be rolled
/// into a single investigation. Stage progression is linear: Open → Investigation →
/// CorrectiveAction → Verification → Closed. Fields are filled in as the case progresses.
/// </summary>
[Table("CapaCase", Schema = "qa")]
public class CapaCase
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [MaxLength(32)] public string CaseNumber { get; set; } = string.Empty;
    [MaxLength(200)] public string Title { get; set; } = string.Empty;

    public CapaStatus Status { get; set; } = CapaStatus.Open;

    [MaxLength(2000)] public string? RootCause { get; set; }
    [MaxLength(2000)] public string? CorrectiveAction { get; set; }
    [MaxLength(2000)] public string? PreventiveAction { get; set; }
    [MaxLength(2000)] public string? VerificationNotes { get; set; }

    /// <summary>FK → <c>HumanResources.Employee.BusinessEntityID</c>.</summary>
    public int? OwnerBusinessEntityId { get; set; }

    public DateTime OpenedAt { get; set; }
    public DateTime? ClosedAt { get; set; }

    public DateTime ModifiedDate { get; set; }
}

public enum CapaStatus : byte
{
    Open = 1,
    Investigation = 2,
    CorrectiveAction = 3,
    Verification = 4,
    Closed = 5,
}
