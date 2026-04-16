using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AWBlazorApp.Features.Person.Domain;

/// <summary>Lookup of person-to-business-entity relationships (Owner, Purchasing Agent, etc.). Maps onto the pre-existing <c>Person.ContactType</c> table in AdventureWorks2022.</summary>
[Table("ContactType", Schema = "Person")]
public class ContactType
{
    [Key]
    [Column("ContactTypeID")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Column("Name")]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    [Column("ModifiedDate")]
    public DateTime ModifiedDate { get; set; }
}
