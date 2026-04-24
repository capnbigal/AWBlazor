using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace AWBlazorApp.Features.Scheduling.LineProductAssignments.Domain;

[Table("LineProductAssignment", Schema = "Scheduling")]
public class LineProductAssignment
{
    [Key, Column("LineProductAssignmentID"), DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    [Column("LocationID")] public short LocationId { get; set; }
    [Column("ProductModelID")] public int ProductModelId { get; set; }
    [Column("IsActive")] public bool IsActive { get; set; } = true;
    [Column("ModifiedDate")] public DateTime ModifiedDate { get; set; }
}
