using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AWBlazorApp.Features.Production.ProductSubcategories.Domain;

/// <summary>Product subcategory (Mountain Bikes, Road Bikes, ...). Maps onto the pre-existing <c>Production.ProductSubcategory</c> table. The <c>ProductCategoryID</c> FK is stored as a plain int — no EF navigation.</summary>
[Table("ProductSubcategory", Schema = "Production")]
public class ProductSubcategory
{
    [Key]
    [Column("ProductSubcategoryID")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    /// <summary>Foreign key to <c>Production.ProductCategory.ProductCategoryID</c>. Not a navigation property — the UI edits the raw id.</summary>
    [Column("ProductCategoryID")]
    public int ProductCategoryId { get; set; }

    [Column("Name")]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    [Column("rowguid")]
    public Guid RowGuid { get; set; }

    [Column("ModifiedDate")]
    public DateTime ModifiedDate { get; set; }
}
