using AWBlazorApp.Shared.Domain;
namespace AWBlazorApp.Features.Purchasing.PurchaseOrderHeaders.Domain;

/// <summary>Audit log for <see cref="PurchaseOrderHeader"/>. EF-managed table <c>dbo.PurchaseOrderHeaderAuditLogs</c>.</summary>
public class PurchaseOrderHeaderAuditLog : AdventureWorksAuditLogBase
{
    public int PurchaseOrderId { get; set; }

    public byte RevisionNumber { get; set; }
    public byte Status { get; set; }
    public int EmployeeId { get; set; }
    public int VendorId { get; set; }
    public int ShipMethodId { get; set; }
    public DateTime OrderDate { get; set; }
    public DateTime? ShipDate { get; set; }
    public decimal SubTotal { get; set; }
    public decimal TaxAmt { get; set; }
    public decimal Freight { get; set; }
    public decimal TotalDue { get; set; }
    public DateTime SourceModifiedDate { get; set; }
}
