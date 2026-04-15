using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AWBlazorApp.Features.AdventureWorks.Domain;

/// <summary>Lookup of why a sale happened (Price, On Promotion, Magazine Advertisement, ...). Maps onto the pre-existing <c>Sales.SalesReason</c> table in AdventureWorks2022.</summary>
[Table("SalesReason", Schema = "Sales")]
public class SalesReason
{
    [Key]
    [Column("SalesReasonID")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Column("Name")]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    /// <summary>Category of reason (Other, Marketing, Promotion). Free-text <c>nvarchar(50)</c> in AdventureWorks.</summary>
    [Column("ReasonType")]
    [MaxLength(50)]
    public string ReasonType { get; set; } = string.Empty;

    [Column("ModifiedDate")]
    public DateTime ModifiedDate { get; set; }
}
