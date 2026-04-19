using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AWBlazorApp.Features.Person.Persons.Domain;

/// <summary>
/// People (employees, customers, vendor contacts, etc.). Maps onto the pre-existing
/// <c>Person.Person</c> table. The PK is <c>BusinessEntityID</c> and is NOT an identity column
/// — it's shared with <c>Person.BusinessEntity</c>, so callers must supply the id explicitly
/// on create.
///
/// The real table also has two XML columns (<c>AdditionalContactInfo</c> and <c>Demographics</c>)
/// which we deliberately do NOT map here. The CRUD UI has no use for them and they would pull
/// in extra EF configuration for XML round-tripping. SQL Server allows both to be NULL on
/// insert, so EF will leave them alone when creating new rows from this app.
/// </summary>
[Table("Person", Schema = "Person")]
public class Person
{
    /// <summary>Shared PK / FK to <c>Person.BusinessEntity.BusinessEntityID</c>. NOT identity.</summary>
    [Key]
    [Column("BusinessEntityID")]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int Id { get; set; }

    /// <summary>One of the SQL CHECK-constrained values: SC, IN, SP, EM, VC, GC.</summary>
    [Column("PersonType")]
    [MaxLength(2)]
    public string PersonType { get; set; } = string.Empty;

    /// <summary>0 = Western (last-name first), 1 = Eastern (first-name first).</summary>
    [Column("NameStyle")]
    public bool NameStyle { get; set; }

    [Column("Title")]
    [MaxLength(8)]
    public string? Title { get; set; }

    [Column("FirstName")]
    [MaxLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [Column("MiddleName")]
    [MaxLength(50)]
    public string? MiddleName { get; set; }

    [Column("LastName")]
    [MaxLength(50)]
    public string LastName { get; set; } = string.Empty;

    [Column("Suffix")]
    [MaxLength(10)]
    public string? Suffix { get; set; }

    /// <summary>0 = no contact, 1 = AdventureWorks-only, 2 = AdventureWorks + partners.</summary>
    [Column("EmailPromotion")]
    public int EmailPromotion { get; set; }

    [Column("rowguid")]
    public Guid RowGuid { get; set; }

    [Column("ModifiedDate")]
    public DateTime ModifiedDate { get; set; }
}
