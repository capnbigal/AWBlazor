using System.ComponentModel.DataAnnotations;

namespace AWBlazorApp.Features.AdventureWorks.Domain;

/// <summary>Audit log for <see cref="SalesOrderDetail"/>. EF-managed table <c>dbo.SalesOrderDetailAuditLogs</c>.</summary>
public class SalesOrderDetailAuditLog : AdventureWorksAuditLogBase
{
    public int SalesOrderId { get; set; }
    public int SalesOrderDetailId { get; set; }

    [MaxLength(25)] public string? CarrierTrackingNumber { get; set; }
    public short OrderQty { get; set; }
    public int ProductId { get; set; }
    public int SpecialOfferId { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal UnitPriceDiscount { get; set; }
    public decimal LineTotal { get; set; }
    public Guid RowGuid { get; set; }
    public DateTime SourceModifiedDate { get; set; }
}
