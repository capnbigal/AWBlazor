using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AWBlazorApp.Data.Entities.AdventureWorks;

/// <summary>Lookup of manufacturing scrap reasons (Color incorrect, Paint failed, etc.). Maps onto the pre-existing <c>Production.ScrapReason</c> table in AdventureWorks2022. PK is a <c>smallint</c>.</summary>
[Table("ScrapReason", Schema = "Production")]
public class ScrapReason
{
    [Key]
    [Column("ScrapReasonID")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public short Id { get; set; }

    [Column("Name")]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    [Column("ModifiedDate")]
    public DateTime ModifiedDate { get; set; }
}
