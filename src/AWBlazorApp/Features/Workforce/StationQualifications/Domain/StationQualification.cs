using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AWBlazorApp.Features.Workforce.StationQualifications.Domain;

/// <summary>
/// Required qualification for working at a station. The clock-in trigger hook walks every row
/// matching <see cref="StationId"/> and emits a <see cref="QualificationAlert"/> for any qual
/// the operator doesn't currently hold (or whose hold has expired).
/// </summary>
[Table("StationQualification", Schema = "wf")]
public class StationQualification
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    /// <summary>FK → <c>org.Station.Id</c>.</summary>
    public int StationId { get; set; }

    public int QualificationId { get; set; }

    /// <summary>Required vs. preferred. Preferred quals don't generate alerts on missing.</summary>
    public bool IsRequired { get; set; } = true;

    public DateTime ModifiedDate { get; set; }
}
