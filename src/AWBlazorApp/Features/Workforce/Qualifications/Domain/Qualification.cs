using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AWBlazorApp.Features.Workforce.Qualifications.Domain;

/// <summary>
/// A skill or certification employees can earn ("Forklift certified", "Operates CNC mill").
/// Granted via <see cref="EmployeeQualification"/> rows; required for stations via
/// <see cref="StationQualification"/>; checked at clock-in by the qualification trigger hook.
/// </summary>
[Table("Qualification", Schema = "wf")]
public class Qualification
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [MaxLength(32)] public string Code { get; set; } = string.Empty;
    [MaxLength(200)] public string Name { get; set; } = string.Empty;
    [MaxLength(2000)] public string? Description { get; set; }

    public QualificationCategory Category { get; set; } = QualificationCategory.Skill;

    public bool IsActive { get; set; } = true;

    public DateTime ModifiedDate { get; set; }
}

public enum QualificationCategory : byte
{
    Skill = 1,
    Certification = 2,
    Safety = 3,
    Compliance = 4,
}
