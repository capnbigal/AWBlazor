using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AWBlazorApp.Features.Sales.Stores.Domain;

/// <summary>Retail store information. Maps onto the pre-existing <c>Sales.Store</c> table. The PK is <c>BusinessEntityID</c> and is <b>not</b> an identity column — it's shared with <c>Person.BusinessEntity</c>, so callers must supply the id explicitly on create. The <c>Demographics</c> XML column is intentionally not mapped.</summary>
[Table("Store", Schema = "Sales")]
public class Store
{
    /// <summary>Shared PK / FK to <c>Person.BusinessEntity.BusinessEntityID</c>. NOT an identity column.</summary>
    [Key]
    [Column("BusinessEntityID")]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int Id { get; set; }

    [Column("Name")]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    /// <summary>FK to <c>Sales.SalesPerson.BusinessEntityID</c>. Null when unassigned.</summary>
    [Column("SalesPersonID")]
    public int? SalesPersonId { get; set; }

    // Demographics (XML) is intentionally not mapped — same pattern as Person.AdditionalContactInfo.

    [Column("rowguid")]
    public Guid RowGuid { get; set; }

    [Column("ModifiedDate")]
    public DateTime ModifiedDate { get; set; }
}
