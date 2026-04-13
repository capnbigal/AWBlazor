using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AWBlazorApp.Data.Entities.AdventureWorks;

/// <summary>
/// Product model — a product family that groups variants. Maps onto the pre-existing
/// <c>Production.ProductModel</c> table.
///
/// The real table also has two XML columns (<c>CatalogDescription</c> and <c>Instructions</c>)
/// which we deliberately do NOT map here, same way we skipped XML columns on
/// <see cref="Person"/>. The CRUD UI has no use for them and SQL Server allows both to be NULL
/// on insert.
/// </summary>
[Table("ProductModel", Schema = "Production")]
public class ProductModel
{
    [Key]
    [Column("ProductModelID")]
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
