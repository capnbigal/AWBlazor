using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AWBlazorApp.Data.Entities.AdventureWorks;

/// <summary>Product culture / locale codes (en, fr, es, ...). Maps onto the pre-existing <c>Production.Culture</c> table in AdventureWorks2022.</summary>
[Table("Culture", Schema = "Production")]
public class Culture
{
    /// <summary>Culture code (fixed-length <c>nchar(6)</c>). This is the primary key — NOT an identity column.</summary>
    [Key]
    [Column("CultureID")]
    [MaxLength(6)]
    public string CultureId { get; set; } = string.Empty;

    [Column("Name")]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    [Column("ModifiedDate")]
    public DateTime ModifiedDate { get; set; }
}
