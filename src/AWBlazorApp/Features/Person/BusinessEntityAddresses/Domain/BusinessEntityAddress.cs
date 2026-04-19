using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.Person.BusinessEntityAddresses.Domain;

/// <summary>
/// Junction linking a <see cref="BusinessEntity"/> to an <see cref="Address"/> with a typed
/// purpose (Home, Shipping, Billing, ...). Maps onto the pre-existing
/// <c>Person.BusinessEntityAddress</c> table. 3-column composite PK =
/// (BusinessEntityID, AddressID, AddressTypeID).
/// </summary>
[PrimaryKey(nameof(BusinessEntityId), nameof(AddressId), nameof(AddressTypeId))]
[Table("BusinessEntityAddress", Schema = "Person")]
public class BusinessEntityAddress
{
    /// <summary>FK to <c>Person.BusinessEntity.BusinessEntityID</c>. Part of the composite PK.</summary>
    [Column("BusinessEntityID")]
    public int BusinessEntityId { get; set; }

    /// <summary>FK to <c>Person.Address.AddressID</c>. Part of the composite PK.</summary>
    [Column("AddressID")]
    public int AddressId { get; set; }

    /// <summary>FK to <c>Person.AddressType.AddressTypeID</c>. Part of the composite PK.</summary>
    [Column("AddressTypeID")]
    public int AddressTypeId { get; set; }

    [Column("rowguid")]
    public Guid RowGuid { get; set; }

    [Column("ModifiedDate")]
    public DateTime ModifiedDate { get; set; }
}
