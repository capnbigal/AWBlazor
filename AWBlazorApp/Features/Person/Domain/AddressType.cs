using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AWBlazorApp.Features.Person.Domain;

/// <summary>Lookup of contact-address categories (Home, Shipping, Billing, etc.). Maps onto the pre-existing <c>Person.AddressType</c> table in AdventureWorks2022; EF reads/writes but never alters it.</summary>
[Table("AddressType", Schema = "Person")]
public class AddressType
{
    [Key]
    [Column("AddressTypeID")]
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
