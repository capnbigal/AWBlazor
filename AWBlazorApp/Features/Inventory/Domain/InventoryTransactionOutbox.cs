using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AWBlazorApp.Features.Inventory.Domain;

/// <summary>
/// Per-transaction JSON-emission queue. One row per <see cref="InventoryTransaction"/> whose
/// type has <c>EmitsJson = true</c>. The Hangfire outbox emitter picks up Pending rows whose
/// <see cref="NextAttemptAt"/> is past, serializes the canonical envelope, and flips the row
/// to Published (or Failed → DeadLetter after too many retries). State, not audit — changes
/// aren't logged because they're high-churn operational data.
/// </summary>
[Table("InventoryTransactionOutbox", Schema = "inv")]
public class InventoryTransactionOutbox
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    public long InventoryTransactionId { get; set; }

    [Column(TypeName = "nvarchar(max)")]
    public string Payload { get; set; } = string.Empty;

    public OutboxStatus Status { get; set; } = OutboxStatus.Pending;

    public int Attempts { get; set; }

    public DateTime? NextAttemptAt { get; set; }

    [MaxLength(2000)]
    public string? LastError { get; set; }

    public DateTime? PublishedAt { get; set; }

    public DateTime CreatedAt { get; set; }
}

public enum OutboxStatus : byte
{
    Pending = 1,
    Publishing = 2,
    Published = 3,
    Failed = 4,
    DeadLetter = 5,
}
