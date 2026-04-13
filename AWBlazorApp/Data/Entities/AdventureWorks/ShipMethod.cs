using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AWBlazorApp.Data.Entities.AdventureWorks;

/// <summary>Shipping carrier / method lookup. Maps onto the pre-existing <c>Purchasing.ShipMethod</c> table.</summary>
[Table("ShipMethod", Schema = "Purchasing")]
public class ShipMethod
{
    [Key]
    [Column("ShipMethodID")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Column("Name")]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    /// <summary>Minimum shipping charge. SQL <c>money</c>.</summary>
    [Column("ShipBase", TypeName = "money")]
    public decimal ShipBase { get; set; }

    /// <summary>Per-unit shipping charge. SQL <c>money</c>.</summary>
    [Column("ShipRate", TypeName = "money")]
    public decimal ShipRate { get; set; }

    [Column("rowguid")]
    public Guid RowGuid { get; set; }

    [Column("ModifiedDate")]
    public DateTime ModifiedDate { get; set; }
}
