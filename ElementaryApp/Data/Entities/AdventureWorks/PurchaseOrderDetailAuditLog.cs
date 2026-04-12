namespace ElementaryApp.Data.Entities.AdventureWorks;

/// <summary>Audit log for <see cref="PurchaseOrderDetail"/>. EF-managed table <c>dbo.PurchaseOrderDetailAuditLogs</c>.</summary>
public class PurchaseOrderDetailAuditLog : AdventureWorksAuditLogBase
{
    public int PurchaseOrderId { get; set; }
    public int PurchaseOrderDetailId { get; set; }

    public DateTime DueDate { get; set; }
    public short OrderQty { get; set; }
    public int ProductId { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
    public decimal ReceivedQty { get; set; }
    public decimal RejectedQty { get; set; }
    public decimal StockedQty { get; set; }
    public DateTime SourceModifiedDate { get; set; }
}
