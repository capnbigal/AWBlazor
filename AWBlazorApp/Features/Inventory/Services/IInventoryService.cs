using AWBlazorApp.Features.Inventory.Dtos;
using AWBlazorApp.Features.Inventory.Adjustments.Domain; using AWBlazorApp.Features.Inventory.Items.Domain; using AWBlazorApp.Features.Inventory.Locations.Domain; using AWBlazorApp.Features.Inventory.Lots.Domain; using AWBlazorApp.Features.Inventory.Outbox.Domain; using AWBlazorApp.Features.Inventory.Queue.Domain; using AWBlazorApp.Features.Inventory.Reports.Domain; using AWBlazorApp.Features.Inventory.Serials.Domain; using AWBlazorApp.Features.Inventory.Transactions.Domain; using AWBlazorApp.Features.Inventory.Types.Domain; 

namespace AWBlazorApp.Features.Inventory.Services;

/// <summary>
/// Single write path for stock movements. UI, endpoints, Hangfire jobs, anything that posts
/// a transaction goes through here. Never write to <see cref="InventoryTransaction"/> or
/// <see cref="InventoryBalance"/> directly — <see cref="PostTransactionAsync"/> owns the
/// validate-insert-upsert-outbox-audit-commit sequence as a single DB transaction so the
/// ledger, the balance aggregate, and the outbox stay in lockstep.
/// </summary>
public interface IInventoryService
{
    Task<PostTransactionResult> PostTransactionAsync(
        PostTransactionRequest request, string? userId, CancellationToken cancellationToken);
}

/// <summary>
/// Input for <see cref="IInventoryService.PostTransactionAsync"/>. Exactly which of
/// <see cref="FromLocationId"/> and <see cref="ToLocationId"/> must be set depends on the
/// transaction type's sign (see service implementation for the rules).
/// </summary>
public sealed record PostTransactionRequest(
    string TypeCode,
    int InventoryItemId,
    decimal Quantity,
    string UnitMeasureCode,
    int? FromLocationId,
    int? ToLocationId,
    int? LotId,
    int? SerialUnitId,
    BalanceStatus? FromStatus,
    BalanceStatus? ToStatus,
    TransactionReferenceKind? ReferenceType,
    int? ReferenceId,
    int? ReferenceLineId,
    string? Notes,
    Guid? CorrelationId,
    DateTime? OccurredAt);

public sealed record PostTransactionResult(
    long TransactionId,
    string TransactionNumber,
    bool OutboxEnqueued);
