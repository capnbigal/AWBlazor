using AWBlazorApp.Shared.Domain;
using System.ComponentModel.DataAnnotations;

namespace AWBlazorApp.Features.Logistics.Domain;

public class StockTransferAuditLog : AdventureWorksAuditLogBase
{
    public int StockTransferId { get; set; }

    [MaxLength(32)] public string? TransferNumber { get; set; }
    public int FromLocationId { get; set; }
    public int ToLocationId { get; set; }
    public int? FromOrganizationId { get; set; }
    public int? ToOrganizationId { get; set; }
    public StockTransferStatus Status { get; set; }
    public Guid? CorrelationId { get; set; }
    public DateTime InitiatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    [MaxLength(450)] public string? PostedByUserId { get; set; }
    [MaxLength(500)] public string? Notes { get; set; }
    public DateTime SourceModifiedDate { get; set; }
}
