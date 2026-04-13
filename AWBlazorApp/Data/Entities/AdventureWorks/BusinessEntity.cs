using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AWBlazorApp.Data.Entities.AdventureWorks;

/// <summary>
/// Root identifier shared by every contactable entity in AdventureWorks (Person, Store, Vendor,
/// SalesPerson, Employee, etc.). Maps onto the pre-existing <c>Person.BusinessEntity</c> table.
/// Almost no data of its own — it exists so the children can share an integer surrogate key.
/// </summary>
[Table("BusinessEntity", Schema = "Person")]
public class BusinessEntity
{
    [Key]
    [Column("BusinessEntityID")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Column("rowguid")]
    public Guid RowGuid { get; set; }

    [Column("ModifiedDate")]
    public DateTime ModifiedDate { get; set; }
}
