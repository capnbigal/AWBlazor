using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AWBlazorApp.Features.AdventureWorks.Domain;

/// <summary>High-level product category (Bikes, Components, Clothing, Accessories). Maps onto the pre-existing <c>Production.ProductCategory</c> table in AdventureWorks2022.</summary>
[Table("ProductCategory", Schema = "Production")]
public class ProductCategory
{
    [Key]
    [Column("ProductCategoryID")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Column("Name")]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    [Column("rowguid")]
    public Guid RowGuid { get; set; }

    [Column("ModifiedDate")]
    public DateTime ModifiedDate { get; set; }
}
