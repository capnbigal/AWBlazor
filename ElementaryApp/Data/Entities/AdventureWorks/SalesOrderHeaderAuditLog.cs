using System.ComponentModel.DataAnnotations;

namespace ElementaryApp.Data.Entities.AdventureWorks;

/// <summary>Audit log for <see cref="SalesOrderHeader"/>. EF-managed table <c>dbo.SalesOrderHeaderAuditLogs</c>.</summary>
public class SalesOrderHeaderAuditLog : AdventureWorksAuditLogBase
{
    public int SalesOrderId { get; set; }

    public byte RevisionNumber { get; set; }
    public DateTime OrderDate { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? ShipDate { get; set; }
    public byte Status { get; set; }
    public bool OnlineOrderFlag { get; set; }
    [MaxLength(25)] public string? SalesOrderNumber { get; set; }
    [MaxLength(25)] public string? PurchaseOrderNumber { get; set; }
    [MaxLength(15)] public string? AccountNumber { get; set; }
    public int CustomerId { get; set; }
    public int? SalesPersonId { get; set; }
    public int? TerritoryId { get; set; }
    public int BillToAddressId { get; set; }
    public int ShipToAddressId { get; set; }
    public int ShipMethodId { get; set; }
    public int? CreditCardId { get; set; }
    [MaxLength(15)] public string? CreditCardApprovalCode { get; set; }
    public int? CurrencyRateId { get; set; }
    public decimal SubTotal { get; set; }
    public decimal TaxAmt { get; set; }
    public decimal Freight { get; set; }
    public decimal TotalDue { get; set; }
    [MaxLength(128)] public string? Comment { get; set; }
    public Guid RowGuid { get; set; }
    public DateTime SourceModifiedDate { get; set; }
}
