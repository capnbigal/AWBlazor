using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ElementaryApp.Data.Entities.AdventureWorks;

/// <summary>
/// Email address attached to a <see cref="Person"/>. A person can have multiple email addresses.
/// Maps onto the pre-existing <c>Person.EmailAddress</c> table. Composite PK =
/// (BusinessEntityID, EmailAddressID). The <c>EmailAddressID</c> is identity-generated within
/// each business entity.
/// </summary>
[PrimaryKey(nameof(BusinessEntityId), nameof(EmailAddressId))]
[Table("EmailAddress", Schema = "Person")]
public class EmailAddress
{
    /// <summary>FK to <c>Person.Person.BusinessEntityID</c>. Part of the composite PK.</summary>
    [Column("BusinessEntityID")]
    public int BusinessEntityId { get; set; }

    /// <summary>SQL identity column scoped per business entity. Part of the composite PK.</summary>
    [Column("EmailAddressID")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int EmailAddressId { get; set; }

    [Column("EmailAddress")]
    [MaxLength(50)]
    public string? EmailAddressValue { get; set; }

    [Column("rowguid")]
    public Guid RowGuid { get; set; }

    [Column("ModifiedDate")]
    public DateTime ModifiedDate { get; set; }
}
