using AWBlazorApp.Features.AdventureWorks.Domain;
using System.ComponentModel.DataAnnotations;

namespace AWBlazorApp.Features.Production.Domain;

/// <summary>Audit log for <see cref="TransactionHistory"/>. EF-managed table <c>dbo.TransactionHistoryAuditLogs</c>.</summary>
public class TransactionHistoryAuditLog : AdventureWorksAuditLogBase
{
    public int TransactionId { get; set; }

    public int ProductId { get; set; }
    public int ReferenceOrderId { get; set; }
    public int ReferenceOrderLineId { get; set; }
    public DateTime TransactionDate { get; set; }
    [MaxLength(1)] public string TransactionType { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal ActualCost { get; set; }
    public DateTime SourceModifiedDate { get; set; }
}
