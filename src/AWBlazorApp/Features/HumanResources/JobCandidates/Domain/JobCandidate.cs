using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AWBlazorApp.Features.HumanResources.JobCandidates.Domain;

/// <summary>
/// Job candidate information. Maps onto the pre-existing <c>HumanResources.JobCandidate</c> table.
///
/// The real table also has a <c>Resume</c> XML column which we deliberately do NOT map here.
/// SQL Server allows it to be NULL on insert, so EF will leave it alone when creating new rows.
/// </summary>
[Table("JobCandidate", Schema = "HumanResources")]
public class JobCandidate
{
    [Key]
    [Column("JobCandidateID")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    /// <summary>FK to <c>HumanResources.Employee.BusinessEntityID</c>. Null when not yet an employee.</summary>
    [Column("BusinessEntityID")]
    public int? BusinessEntityId { get; set; }

    [Column("ModifiedDate")]
    public DateTime ModifiedDate { get; set; }
}
