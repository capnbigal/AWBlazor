using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AWBlazorApp.Features.Logistics.Domain;

/// <summary>
/// Inter-location or inter-org stock move. Covers the user's Distribution case as well: when
/// <see cref="FromOrganizationId"/> and <see cref="ToOrganizationId"/> differ, this is an
/// inter-org transfer. Posting writes paired <c>TRANSFER_OUT</c> + <c>TRANSFER_IN</c>
/// inventory transactions sharing <see cref="CorrelationId"/>, so history can walk the two
/// legs as a single event.
/// </summary>
[Table("StockTransfer", Schema = "lgx")]
public class StockTransfer
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [MaxLength(32)]
    public string TransferNumber { get; set; } = string.Empty;

    public int FromLocationId { get; set; }
    public int ToLocationId { get; set; }

    public int? FromOrganizationId { get; set; }
    public int? ToOrganizationId { get; set; }

    public StockTransferStatus Status { get; set; } = StockTransferStatus.Draft;

    /// <summary>Set when the transfer is posted. Both the OUT and IN inventory transactions
    /// share this value so the ledger can pair the two legs.</summary>
    public Guid? CorrelationId { get; set; }

    public DateTime InitiatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    [MaxLength(450)]
    public string? PostedByUserId { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }

    public DateTime ModifiedDate { get; set; }
}

public enum StockTransferStatus : byte
{
    Draft = 1,
    InTransit = 2,
    Completed = 3,
    Cancelled = 4,
}
