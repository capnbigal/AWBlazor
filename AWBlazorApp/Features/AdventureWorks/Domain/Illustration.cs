using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AWBlazorApp.Features.AdventureWorks.Domain;

/// <summary>
/// Bicycle assembly diagrams. Maps onto the pre-existing <c>Production.Illustration</c> table.
///
/// The real table has a <c>Diagram</c> XML column which we deliberately do NOT map here. The
/// CRUD UI has no use for raw SVG-style XML diagrams; SQL Server allows the column to be NULL
/// on insert from this app.
/// </summary>
[Table("Illustration", Schema = "Production")]
public class Illustration
{
    [Key]
    [Column("IllustrationID")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Column("ModifiedDate")]
    public DateTime ModifiedDate { get; set; }
}
