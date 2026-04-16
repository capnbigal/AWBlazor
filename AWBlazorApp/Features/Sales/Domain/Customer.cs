using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AWBlazorApp.Features.Sales.Domain;

/// <summary>Sales customer linking a <see cref="Store"/> or a <see cref="Person"/> to a territory. Maps onto the pre-existing <c>Sales.Customer</c> table. <c>AccountNumber</c> is a computed column in SQL (<c>'AW' + leading-zeros(CustomerID)</c>) — EF never writes it.</summary>
[Table("Customer", Schema = "Sales")]
public class Customer
{
    [Key]
    [Column("CustomerID")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    /// <summary>Foreign key to <c>Person.Person.BusinessEntityID</c>. Null when the customer is a store.</summary>
    [Column("PersonID")]
    public int? PersonId { get; set; }

    /// <summary>Foreign key to <c>Sales.Store.BusinessEntityID</c>. Null when the customer is a person.</summary>
    [Column("StoreID")]
    public int? StoreId { get; set; }

    /// <summary>Foreign key to <c>Sales.SalesTerritory.TerritoryID</c>.</summary>
    [Column("TerritoryID")]
    public int? TerritoryId { get; set; }

    /// <summary>Computed by SQL Server on insert/update; read-only from EF's perspective.</summary>
    [Column("AccountNumber")]
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    [MaxLength(10)]
    public string AccountNumber { get; set; } = string.Empty;

    [Column("rowguid")]
    public Guid RowGuid { get; set; }

    [Column("ModifiedDate")]
    public DateTime ModifiedDate { get; set; }
}
