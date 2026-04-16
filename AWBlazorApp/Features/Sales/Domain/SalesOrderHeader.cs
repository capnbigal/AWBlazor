using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AWBlazorApp.Features.Sales.Domain;

/// <summary>Sales order header. Maps onto the pre-existing <c>Sales.SalesOrderHeader</c> table — one of the largest tables in AdventureWorks. <c>SalesOrderNumber</c> and <c>TotalDue</c> are SQL-computed columns.</summary>
[Table("SalesOrderHeader", Schema = "Sales")]
public class SalesOrderHeader
{
    [Key]
    [Column("SalesOrderID")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Column("RevisionNumber")]
    public byte RevisionNumber { get; set; }

    [Column("OrderDate")]
    public DateTime OrderDate { get; set; }

    [Column("DueDate")]
    public DateTime DueDate { get; set; }

    [Column("ShipDate")]
    public DateTime? ShipDate { get; set; }

    /// <summary>1=InProcess, 2=Approved, 3=Backordered, 4=Rejected, 5=Shipped, 6=Cancelled.</summary>
    [Column("Status")]
    public byte Status { get; set; }

    [Column("OnlineOrderFlag")]
    public bool OnlineOrderFlag { get; set; }

    /// <summary>Computed by SQL Server — read-only from EF's perspective.</summary>
    [Column("SalesOrderNumber")]
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    [MaxLength(25)]
    public string SalesOrderNumber { get; set; } = string.Empty;

    [Column("PurchaseOrderNumber")]
    [MaxLength(25)]
    public string? PurchaseOrderNumber { get; set; }

    [Column("AccountNumber")]
    [MaxLength(15)]
    public string? AccountNumber { get; set; }

    /// <summary>FK to <c>Sales.Customer.CustomerID</c>.</summary>
    [Column("CustomerID")]
    public int CustomerId { get; set; }

    /// <summary>FK to <c>Sales.SalesPerson.BusinessEntityID</c>.</summary>
    [Column("SalesPersonID")]
    public int? SalesPersonId { get; set; }

    /// <summary>FK to <c>Sales.SalesTerritory.TerritoryID</c>.</summary>
    [Column("TerritoryID")]
    public int? TerritoryId { get; set; }

    /// <summary>FK to <c>Person.Address.AddressID</c> (bill-to).</summary>
    [Column("BillToAddressID")]
    public int BillToAddressId { get; set; }

    /// <summary>FK to <c>Person.Address.AddressID</c> (ship-to).</summary>
    [Column("ShipToAddressID")]
    public int ShipToAddressId { get; set; }

    /// <summary>FK to <c>Purchasing.ShipMethod.ShipMethodID</c>.</summary>
    [Column("ShipMethodID")]
    public int ShipMethodId { get; set; }

    /// <summary>FK to <c>Sales.CreditCard.CreditCardID</c>.</summary>
    [Column("CreditCardID")]
    public int? CreditCardId { get; set; }

    [Column("CreditCardApprovalCode")]
    [MaxLength(15)]
    public string? CreditCardApprovalCode { get; set; }

    /// <summary>FK to <c>Sales.CurrencyRate.CurrencyRateID</c>.</summary>
    [Column("CurrencyRateID")]
    public int? CurrencyRateId { get; set; }

    [Column("SubTotal", TypeName = "money")]
    public decimal SubTotal { get; set; }

    [Column("TaxAmt", TypeName = "money")]
    public decimal TaxAmt { get; set; }

    [Column("Freight", TypeName = "money")]
    public decimal Freight { get; set; }

    /// <summary>Computed by SQL Server (<c>SubTotal + TaxAmt + Freight</c>) — read-only from EF's perspective.</summary>
    [Column("TotalDue", TypeName = "money")]
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public decimal TotalDue { get; set; }

    [Column("Comment")]
    [MaxLength(128)]
    public string? Comment { get; set; }

    [Column("rowguid")]
    public Guid RowGuid { get; set; }

    [Column("ModifiedDate")]
    public DateTime ModifiedDate { get; set; }
}
