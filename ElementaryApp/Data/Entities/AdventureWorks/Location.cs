using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElementaryApp.Data.Entities.AdventureWorks;

/// <summary>Manufacturing location (Tool Crib, Paint Shop, ...). Maps onto the pre-existing <c>Production.Location</c> table. PK is <c>smallint</c>.</summary>
[Table("Location", Schema = "Production")]
public class Location
{
    [Key]
    [Column("LocationID")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public short Id { get; set; }

    [Column("Name")]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    /// <summary>Hourly cost rate. SQL <c>smallmoney</c>.</summary>
    [Column("CostRate", TypeName = "smallmoney")]
    public decimal CostRate { get; set; }

    /// <summary>Work capacity (hours) per day. SQL <c>decimal(8,2)</c>.</summary>
    [Column("Availability", TypeName = "decimal(8,2)")]
    public decimal Availability { get; set; }

    [Column("ModifiedDate")]
    public DateTime ModifiedDate { get; set; }
}
