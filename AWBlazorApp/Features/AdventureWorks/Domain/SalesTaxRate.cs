using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AWBlazorApp.Features.AdventureWorks.Domain;

/// <summary>Sales tax rate applied per state/province. Maps onto the pre-existing <c>Sales.SalesTaxRate</c> table. <c>StateProvinceID</c> is an FK stored as a plain int.</summary>
[Table("SalesTaxRate", Schema = "Sales")]
public class SalesTaxRate
{
    [Key]
    [Column("SalesTaxRateID")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    /// <summary>Foreign key to <c>Person.StateProvince.StateProvinceID</c>.</summary>
    [Column("StateProvinceID")]
    public int StateProvinceId { get; set; }

    /// <summary>Tax category: 1 = State or province, 2 = Federal, 3 = Shared.</summary>
    [Column("TaxType")]
    public byte TaxType { get; set; }

    /// <summary>Tax rate (percentage). SQL <c>smallmoney</c>.</summary>
    [Column("TaxRate", TypeName = "smallmoney")]
    public decimal TaxRate { get; set; }

    [Column("Name")]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    [Column("rowguid")]
    public Guid RowGuid { get; set; }

    [Column("ModifiedDate")]
    public DateTime ModifiedDate { get; set; }
}
