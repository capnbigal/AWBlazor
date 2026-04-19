using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AWBlazorApp.Features.Purchasing.PurchaseOrderHeaders.Domain;

/// <summary>Purchase order header. Maps onto the pre-existing <c>Purchasing.PurchaseOrderHeader</c> table. <c>TotalDue</c> is a computed column (<c>SubTotal + TaxAmt + Freight</c>) — EF never writes it.</summary>
[Table("PurchaseOrderHeader", Schema = "Purchasing")]
public class PurchaseOrderHeader
{
    [Key]
    [Column("PurchaseOrderID")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Column("RevisionNumber")]
    public byte RevisionNumber { get; set; }

    /// <summary>1=Pending, 2=Approved, 3=Rejected, 4=Complete.</summary>
    [Column("Status")]
    public byte Status { get; set; }

    /// <summary>FK to <c>HumanResources.Employee.BusinessEntityID</c>.</summary>
    [Column("EmployeeID")]
    public int EmployeeId { get; set; }

    /// <summary>FK to <c>Purchasing.Vendor.BusinessEntityID</c>.</summary>
    [Column("VendorID")]
    public int VendorId { get; set; }

    /// <summary>FK to <c>Purchasing.ShipMethod.ShipMethodID</c>.</summary>
    [Column("ShipMethodID")]
    public int ShipMethodId { get; set; }

    [Column("OrderDate")]
    public DateTime OrderDate { get; set; }

    [Column("ShipDate")]
    public DateTime? ShipDate { get; set; }

    [Column("SubTotal", TypeName = "money")]
    public decimal SubTotal { get; set; }

    [Column("TaxAmt", TypeName = "money")]
    public decimal TaxAmt { get; set; }

    [Column("Freight", TypeName = "money")]
    public decimal Freight { get; set; }

    /// <summary>Computed in SQL as <c>SubTotal + TaxAmt + Freight</c>. Read-only.</summary>
    [Column("TotalDue", TypeName = "money")]
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public decimal TotalDue { get; set; }

    [Column("ModifiedDate")]
    public DateTime ModifiedDate { get; set; }
}
