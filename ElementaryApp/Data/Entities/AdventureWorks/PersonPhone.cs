using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ElementaryApp.Data.Entities.AdventureWorks;

/// <summary>
/// Phone number attached to a <see cref="Person"/>. A person can have multiple phone numbers,
/// each scoped by a phone-number type (Cell, Home, Work, ...). Maps onto the pre-existing
/// <c>Person.PersonPhone</c> table. Composite PK = (BusinessEntityID, PhoneNumber, PhoneNumberTypeID).
/// </summary>
[PrimaryKey(nameof(BusinessEntityId), nameof(PhoneNumber), nameof(PhoneNumberTypeId))]
[Table("PersonPhone", Schema = "Person")]
public class PersonPhone
{
    /// <summary>FK to <c>Person.Person.BusinessEntityID</c>. Part of the composite PK.</summary>
    [Column("BusinessEntityID")]
    public int BusinessEntityId { get; set; }

    /// <summary>The phone number itself. Part of the composite PK — the natural key.</summary>
    [Column("PhoneNumber")]
    [MaxLength(25)]
    public string PhoneNumber { get; set; } = string.Empty;

    /// <summary>FK to <c>Person.PhoneNumberType.PhoneNumberTypeID</c>. Part of the composite PK.</summary>
    [Column("PhoneNumberTypeID")]
    public int PhoneNumberTypeId { get; set; }

    [Column("ModifiedDate")]
    public DateTime ModifiedDate { get; set; }
}
