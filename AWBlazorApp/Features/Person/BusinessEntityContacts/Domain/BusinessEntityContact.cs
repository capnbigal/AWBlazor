using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.Person.BusinessEntityContacts.Domain;

/// <summary>
/// Junction linking a <see cref="BusinessEntity"/> to a <see cref="Person"/> with a typed
/// contact role (Owner, Order Administrator, ...). Maps onto the pre-existing
/// <c>Person.BusinessEntityContact</c> table. 3-column composite PK =
/// (BusinessEntityID, PersonID, ContactTypeID).
/// </summary>
[PrimaryKey(nameof(BusinessEntityId), nameof(PersonId), nameof(ContactTypeId))]
[Table("BusinessEntityContact", Schema = "Person")]
public class BusinessEntityContact
{
    /// <summary>FK to <c>Person.BusinessEntity.BusinessEntityID</c>. Part of the composite PK.</summary>
    [Column("BusinessEntityID")]
    public int BusinessEntityId { get; set; }

    /// <summary>FK to <c>Person.Person.BusinessEntityID</c>. Part of the composite PK.</summary>
    [Column("PersonID")]
    public int PersonId { get; set; }

    /// <summary>FK to <c>Person.ContactType.ContactTypeID</c>. Part of the composite PK.</summary>
    [Column("ContactTypeID")]
    public int ContactTypeId { get; set; }

    [Column("rowguid")]
    public Guid RowGuid { get; set; }

    [Column("ModifiedDate")]
    public DateTime ModifiedDate { get; set; }
}
