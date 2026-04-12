namespace ElementaryApp.Data.Entities.AdventureWorks;

/// <summary>Audit log for <see cref="SalesOrderHeaderSalesReason"/>. EF-managed table <c>dbo.SalesOrderHeaderSalesReasonAuditLogs</c>. A pure junction table — audit rows only carry the composite key plus timestamp.</summary>
public class SalesOrderHeaderSalesReasonAuditLog : AdventureWorksAuditLogBase
{
    public int SalesOrderId { get; set; }
    public int SalesReasonId { get; set; }

    public DateTime SourceModifiedDate { get; set; }
}
