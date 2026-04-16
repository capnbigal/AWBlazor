using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AWBlazorApp.Features.Person.Domain;

/// <summary>ISO country / region codes. Maps onto the pre-existing <c>Person.CountryRegion</c> table in AdventureWorks2022.</summary>
[Table("CountryRegion", Schema = "Person")]
public class CountryRegion
{
    /// <summary>ISO standard country/region code (up to 3 chars). This is the primary key — NOT an identity column.</summary>
    [Key]
    [Column("CountryRegionCode")]
    [MaxLength(3)]
    public string CountryRegionCode { get; set; } = string.Empty;

    [Column("Name")]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    [Column("ModifiedDate")]
    public DateTime ModifiedDate { get; set; }
}
