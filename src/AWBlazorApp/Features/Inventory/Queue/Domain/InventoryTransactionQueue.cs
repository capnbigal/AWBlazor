using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AWBlazorApp.Features.Inventory.Queue.Domain;

/// <summary>
/// Inbound queue for transactions arriving from outside the app — future AS2 receipt, manual
/// imports, direct API clients. Two-stage state: parse the <see cref="RawPayload"/> into a
/// real transaction, then post it. On successful post, <see cref="PostedTransactionId"/>
/// points at the resulting ledger row.
/// </summary>
[Table("InventoryTransactionQueue", Schema = "inv")]
public class InventoryTransactionQueue
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    public TransactionQueueSource Source { get; set; }

    [Column(TypeName = "nvarchar(max)")]
    public string RawPayload { get; set; } = string.Empty;

    public QueueParseStatus ParseStatus { get; set; } = QueueParseStatus.Pending;
    public QueueProcessStatus ProcessStatus { get; set; } = QueueProcessStatus.Pending;

    public int Attempts { get; set; }

    [MaxLength(2000)]
    public string? LastError { get; set; }

    public DateTime ReceivedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }

    public long? PostedTransactionId { get; set; }
}

public enum TransactionQueueSource : byte
{
    AS2 = 1,
    Manual = 2,
    Import = 3,
    Api = 4,
}

public enum QueueParseStatus : byte
{
    Pending = 1,
    Parsed = 2,
    Failed = 3,
}

public enum QueueProcessStatus : byte
{
    Pending = 1,
    Posted = 2,
    Rejected = 3,
}
