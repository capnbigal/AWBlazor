using System.ComponentModel.DataAnnotations;

namespace ElementaryApp.Data.Entities.AdventureWorks;

/// <summary>Audit log for <see cref="TransactionHistoryArchive"/>. EF-managed table <c>dbo.TransactionHistoryArchiveAuditLogs</c>.</summary>
public class TransactionHistoryArchiveAuditLog : AdventureWorksAuditLogBase
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
