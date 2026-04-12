using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElementaryApp.Data.Entities.AdventureWorks;

/// <summary>Product marketing description. Maps onto the pre-existing <c>Production.ProductDescription</c> table.</summary>
[Table("ProductDescription", Schema = "Production")]
public class ProductDescription
{
    [Key]
    [Column("ProductDescriptionID")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    /// <summary>Marketing description (up to 400 characters).</summary>
    [Column("Description")]
    [MaxLength(400)]
    public string Description { get; set; } = string.Empty;

    [Column("rowguid")]
    public Guid RowGuid { get; set; }

    [Column("ModifiedDate")]
    public DateTime ModifiedDate { get; set; }
}
