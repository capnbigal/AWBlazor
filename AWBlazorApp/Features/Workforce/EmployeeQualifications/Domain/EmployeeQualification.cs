using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AWBlazorApp.Features.Workforce.EmployeeQualifications.Domain;

/// <summary>
/// An employee's hold on a qualification. <see cref="EarnedDate"/> is when it was first
/// awarded; <see cref="ExpiresOn"/> is the next renewal cutoff (null = never expires).
/// The UI flags rows as "Expired" or "Soon" based on ExpiresOn vs. now.
/// </summary>
[Table("EmployeeQualification", Schema = "wf")]
public class EmployeeQualification
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    /// <summary>FK → <c>HumanResources.Employee.BusinessEntityID</c>.</summary>
    public int BusinessEntityId { get; set; }

    public int QualificationId { get; set; }

    public DateTime EarnedDate { get; set; }
    public DateTime? ExpiresOn { get; set; }

    [MaxLength(500)] public string? EvidenceUrl { get; set; }
    [MaxLength(450)] public string? VerifiedByUserId { get; set; }

    [MaxLength(500)] public string? Notes { get; set; }

    public DateTime ModifiedDate { get; set; }
}
