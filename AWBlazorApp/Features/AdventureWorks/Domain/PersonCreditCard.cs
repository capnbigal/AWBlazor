using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.AdventureWorks.Domain;

/// <summary>Junction linking persons to credit cards. Maps onto the pre-existing <c>Sales.PersonCreditCard</c> table. Composite PK = (BusinessEntityID, CreditCardID).</summary>
[PrimaryKey(nameof(BusinessEntityId), nameof(CreditCardId))]
[Table("PersonCreditCard", Schema = "Sales")]
public class PersonCreditCard
{
    /// <summary>FK to <c>Person.BusinessEntity.BusinessEntityID</c>. Part of the composite PK.</summary>
    [Column("BusinessEntityID")]
    public int BusinessEntityId { get; set; }

    /// <summary>FK to <c>Sales.CreditCard.CreditCardID</c>. Part of the composite PK.</summary>
    [Column("CreditCardID")]
    public int CreditCardId { get; set; }

    [Column("ModifiedDate")]
    public DateTime ModifiedDate { get; set; }
}
