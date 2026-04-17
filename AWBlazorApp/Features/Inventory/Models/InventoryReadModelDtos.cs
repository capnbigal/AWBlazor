using AWBlazorApp.Features.Inventory.Domain;

namespace AWBlazorApp.Features.Inventory.Models;

// Read-only DTOs for the append-only ledger, derived balances, seed reference table, and the
// two operational queues. Kept in a single file because none of them need mapping helpers
// beyond a trivial ToDto — they're never created or updated through an API.

public sealed record InventoryBalanceDto(
    int Id, int InventoryItemId, int LocationId, int? LotId, BalanceStatus Status,
    decimal Quantity, DateTime? LastCountedAt, DateTime? LastTransactionAt);

public sealed record InventoryTransactionTypeDto(
    int Id, string Code, string Name, sbyte Sign, bool RequiresApproval, bool EmitsJson, bool IsActive);

public sealed record InventoryTransactionDto(
    long Id, string TransactionNumber, int TransactionTypeId, DateTime OccurredAt, DateTime PostedAt,
    string? PostedByUserId, int InventoryItemId, int? FromLocationId, int? ToLocationId,
    int? LotId, int? SerialUnitId, decimal Quantity, string UnitMeasureCode,
    BalanceStatus? FromStatus, BalanceStatus? ToStatus,
    TransactionReferenceKind? ReferenceType, int? ReferenceId, int? ReferenceLineId,
    string? Notes, Guid? CorrelationId);

public sealed record PostInventoryTransactionRequest
{
    public string? TypeCode { get; set; }
    public int InventoryItemId { get; set; }
    public decimal Quantity { get; set; }
    public string? UnitMeasureCode { get; set; }
    public int? FromLocationId { get; set; }
    public int? ToLocationId { get; set; }
    public int? LotId { get; set; }
    public int? SerialUnitId { get; set; }
    public BalanceStatus? FromStatus { get; set; }
    public BalanceStatus? ToStatus { get; set; }
    public TransactionReferenceKind? ReferenceType { get; set; }
    public int? ReferenceId { get; set; }
    public int? ReferenceLineId { get; set; }
    public string? Notes { get; set; }
    public Guid? CorrelationId { get; set; }
    public DateTime? OccurredAt { get; set; }
}

public sealed record InventoryOutboxDto(
    long Id, long InventoryTransactionId, string Payload, OutboxStatus Status,
    int Attempts, DateTime? NextAttemptAt, string? LastError, DateTime? PublishedAt, DateTime CreatedAt);

public sealed record InventoryQueueDto(
    long Id, TransactionQueueSource Source, string RawPayload,
    QueueParseStatus ParseStatus, QueueProcessStatus ProcessStatus,
    int Attempts, string? LastError, DateTime ReceivedAt, DateTime? ProcessedAt,
    long? PostedTransactionId);

public static class InventoryReadModelMappings
{
    public static InventoryBalanceDto ToDto(this InventoryBalance e) => new(
        e.Id, e.InventoryItemId, e.LocationId, e.LotId, e.Status, e.Quantity,
        e.LastCountedAt, e.LastTransactionAt);

    public static InventoryTransactionTypeDto ToDto(this InventoryTransactionType e) => new(
        e.Id, e.Code, e.Name, e.Sign, e.RequiresApproval, e.EmitsJson, e.IsActive);

    public static InventoryTransactionDto ToDto(this InventoryTransaction e) => new(
        e.Id, e.TransactionNumber, e.TransactionTypeId, e.OccurredAt, e.PostedAt, e.PostedByUserId,
        e.InventoryItemId, e.FromLocationId, e.ToLocationId, e.LotId, e.SerialUnitId,
        e.Quantity, e.UnitMeasureCode, e.FromStatus, e.ToStatus,
        e.ReferenceType, e.ReferenceId, e.ReferenceLineId, e.Notes, e.CorrelationId);

    public static InventoryOutboxDto ToDto(this InventoryTransactionOutbox e) => new(
        e.Id, e.InventoryTransactionId, e.Payload, e.Status, e.Attempts,
        e.NextAttemptAt, e.LastError, e.PublishedAt, e.CreatedAt);

    public static InventoryQueueDto ToDto(this InventoryTransactionQueue e) => new(
        e.Id, e.Source, e.RawPayload, e.ParseStatus, e.ProcessStatus,
        e.Attempts, e.LastError, e.ReceivedAt, e.ProcessedAt, e.PostedTransactionId);
}
