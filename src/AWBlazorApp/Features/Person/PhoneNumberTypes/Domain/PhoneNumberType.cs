using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AWBlazorApp.Features.Person.PhoneNumberTypes.Domain;

/// <summary>Lookup of phone-number categories (Cell, Home, Work, etc.). Maps onto the pre-existing <c>Person.PhoneNumberType</c> table in AdventureWorks2022.</summary>
[Table("PhoneNumberType", Schema = "Person")]
public class PhoneNumberType
{
    [Key]
    [Column("PhoneNumberTypeID")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Column("Name")]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    [Column("ModifiedDate")]
    public DateTime ModifiedDate { get; set; }
}
