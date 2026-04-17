using System.Text.Json;
using AWBlazorApp.Features.Inventory.Domain;
using AWBlazorApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.Inventory.Services;

/// <inheritdoc />
public sealed class InventoryService(
    IDbContextFactory<ApplicationDbContext> dbFactory,
    ILogger<InventoryService> logger) : IInventoryService
{
    public async Task<PostTransactionResult> PostTransactionAsync(
        PostTransactionRequest request, string? userId, CancellationToken ct)
    {
        if (request.Quantity <= 0)
            throw new InvalidOperationException("Quantity must be positive; sign is derived from the transaction type.");

        await using var db = await dbFactory.CreateDbContextAsync(ct);

        var type = await db.InventoryTransactionTypes.AsNoTracking()
            .FirstOrDefaultAsync(t => t.Code == request.TypeCode, ct)
            ?? throw new InvalidOperationException($"Unknown transaction type '{request.TypeCode}'.");
        if (!type.IsActive)
            throw new InvalidOperationException($"Transaction type '{request.TypeCode}' is inactive.");

        ValidateLocations(type, request);

        var item = await db.InventoryItems.AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == request.InventoryItemId, ct)
            ?? throw new InvalidOperationException($"InventoryItem {request.InventoryItemId} not found.");
        if (!item.IsActive)
            throw new InvalidOperationException($"InventoryItem {request.InventoryItemId} is inactive.");
        if (item.TracksLot && request.LotId is null)
            throw new InvalidOperationException($"InventoryItem {item.Id} tracks lots; LotId is required.");
        if (item.TracksSerial && request.SerialUnitId is null && type.Sign != 0)
            throw new InvalidOperationException($"InventoryItem {item.Id} tracks serial numbers; SerialUnitId is required.");

        await using var tx = await db.Database.BeginTransactionAsync(ct);

        var now = DateTime.UtcNow;
        var transaction = new InventoryTransaction
        {
            TransactionNumber = $"TXN-{now:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..12].ToUpperInvariant()}",
            TransactionTypeId = type.Id,
            OccurredAt = request.OccurredAt ?? now,
            PostedAt = now,
            PostedByUserId = userId,
            InventoryItemId = request.InventoryItemId,
            FromLocationId = request.FromLocationId,
            ToLocationId = request.ToLocationId,
            LotId = request.LotId,
            SerialUnitId = request.SerialUnitId,
            Quantity = request.Quantity,
            UnitMeasureCode = request.UnitMeasureCode,
            FromStatus = request.FromStatus,
            ToStatus = request.ToStatus,
            ReferenceType = request.ReferenceType,
            ReferenceId = request.ReferenceId,
            ReferenceLineId = request.ReferenceLineId,
            Notes = request.Notes,
            CorrelationId = request.CorrelationId,
        };
        db.InventoryTransactions.Add(transaction);
        await db.SaveChangesAsync(ct);

        await UpsertBalanceAsync(db, type, request, transaction.Id, ct);
        await UpdateSerialUnitAsync(db, type, request, ct);

        var outboxEnqueued = false;
        if (type.EmitsJson)
        {
            var envelope = await BuildEnvelopeAsync(db, transaction, type, ct);
            db.InventoryTransactionOutbox.Add(new InventoryTransactionOutbox
            {
                InventoryTransactionId = transaction.Id,
                Payload = JsonSerializer.Serialize(envelope, JsonOptions),
                Status = OutboxStatus.Pending,
                CreatedAt = now,
                NextAttemptAt = now,
            });
            outboxEnqueued = true;
        }

        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        logger.LogInformation(
            "Posted inventory transaction {Number} (type {Code}, item {Item}, qty {Qty}). Outbox: {Outbox}",
            transaction.TransactionNumber, type.Code, item.Id, request.Quantity, outboxEnqueued);

        return new PostTransactionResult(transaction.Id, transaction.TransactionNumber, outboxEnqueued);
    }

    private static void ValidateLocations(InventoryTransactionType type, PostTransactionRequest r)
    {
        switch (type.Sign)
        {
            case > 0 when r.ToLocationId is null:
                throw new InvalidOperationException($"{type.Code}: ToLocationId is required for a positive-sign transaction.");
            case < 0 when r.FromLocationId is null:
                throw new InvalidOperationException($"{type.Code}: FromLocationId is required for a negative-sign transaction.");
            case 0 when r.FromLocationId is null || r.ToLocationId is null:
                throw new InvalidOperationException(
                    $"{type.Code}: both FromLocationId and ToLocationId are required for a paired-move transaction (absolute-count types are not supported via PostTransactionAsync).");
        }
    }

    private static async Task UpsertBalanceAsync(
        ApplicationDbContext db, InventoryTransactionType type, PostTransactionRequest r, long transactionId, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        switch (type.Sign)
        {
            case > 0:
                await AdjustBalanceAsync(db, r.InventoryItemId, r.ToLocationId!.Value, r.LotId,
                    r.ToStatus ?? BalanceStatus.Available, +r.Quantity, now, ct);
                break;
            case < 0:
                await AdjustBalanceAsync(db, r.InventoryItemId, r.FromLocationId!.Value, r.LotId,
                    r.FromStatus ?? BalanceStatus.Available, -r.Quantity, now, ct);
                break;
            case 0:
                await AdjustBalanceAsync(db, r.InventoryItemId, r.FromLocationId!.Value, r.LotId,
                    r.FromStatus ?? BalanceStatus.Available, -r.Quantity, now, ct);
                await AdjustBalanceAsync(db, r.InventoryItemId, r.ToLocationId!.Value, r.LotId,
                    r.ToStatus ?? BalanceStatus.Available, +r.Quantity, now, ct);
                break;
        }
        _ = transactionId;
    }

    private static async Task AdjustBalanceAsync(
        ApplicationDbContext db, int itemId, int locationId, int? lotId,
        BalanceStatus status, decimal delta, DateTime now, CancellationToken ct)
    {
        var existing = await db.InventoryBalances.FirstOrDefaultAsync(
            b => b.InventoryItemId == itemId
              && b.LocationId == locationId
              && b.LotId == lotId
              && b.Status == status, ct);

        if (existing is null)
        {
            db.InventoryBalances.Add(new InventoryBalance
            {
                InventoryItemId = itemId,
                LocationId = locationId,
                LotId = lotId,
                Status = status,
                Quantity = delta,
                LastTransactionAt = now,
            });
        }
        else
        {
            existing.Quantity += delta;
            existing.LastTransactionAt = now;
        }
    }

    private static async Task UpdateSerialUnitAsync(
        ApplicationDbContext db, InventoryTransactionType type, PostTransactionRequest r, CancellationToken ct)
    {
        if (r.SerialUnitId is null) return;

        var serial = await db.SerialUnits.FirstOrDefaultAsync(s => s.Id == r.SerialUnitId, ct);
        if (serial is null) return;

        // Outbound legs update location to the "from" side disappearing; inbound legs point at
        // the new home. For a paired move, the "to" side wins.
        if (r.ToLocationId.HasValue) serial.CurrentLocationId = r.ToLocationId;
        else if (type.Sign < 0 && r.FromLocationId.HasValue) serial.CurrentLocationId = null;

        serial.Status = MapSerialStatus(type, serial.Status);
        serial.ModifiedDate = DateTime.UtcNow;
    }

    private static SerialUnitStatus MapSerialStatus(InventoryTransactionType type, SerialUnitStatus current) => type.Code switch
    {
        InventoryTransactionTypeCodes.Ship          => SerialUnitStatus.Shipped,
        InventoryTransactionTypeCodes.Scrap         => SerialUnitStatus.Scrapped,
        InventoryTransactionTypeCodes.ReturnCust    => SerialUnitStatus.Returned,
        InventoryTransactionTypeCodes.WipIssue      => SerialUnitStatus.Issued,
        InventoryTransactionTypeCodes.Receipt       => SerialUnitStatus.InStock,
        InventoryTransactionTypeCodes.WipReceipt    => SerialUnitStatus.InStock,
        _                                           => current,
    };

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web) { WriteIndented = false };

    /// <summary>
    /// Canonical JSON envelope per the Phase B plan: <c>{header, item, quantity, location,
    /// traceability, references, audit}</c>. Kept as anonymous objects so the shape stays close
    /// to the approved spec and is easy to inspect in the outbox viewer.
    /// </summary>
    private static async Task<object> BuildEnvelopeAsync(
        ApplicationDbContext db, InventoryTransaction tx, InventoryTransactionType type, CancellationToken ct)
    {
        var fromCode = await LookupLocationCodeAsync(db, tx.FromLocationId, ct);
        var toCode = await LookupLocationCodeAsync(db, tx.ToLocationId, ct);
        var lotCode = await LookupLotCodeAsync(db, tx.LotId, ct);
        var serialNumber = await LookupSerialAsync(db, tx.SerialUnitId, ct);

        return new
        {
            header = new
            {
                transactionId = tx.TransactionNumber,
                type = type.Code,
                occurredAt = tx.OccurredAt,
                postedBy = tx.PostedByUserId,
                correlationId = tx.CorrelationId,
            },
            item = new
            {
                inventoryItemId = tx.InventoryItemId,
                uom = tx.UnitMeasureCode,
            },
            quantity = new
            {
                value = tx.Quantity,
                sign = type.Sign,
            },
            location = new
            {
                from = fromCode is null ? null : new { code = fromCode },
                to = toCode is null ? null : new { code = toCode },
            },
            traceability = new
            {
                lot = lotCode,
                serials = serialNumber is null ? Array.Empty<string>() : new[] { serialNumber },
            },
            references = tx.ReferenceType is null ? null : new
            {
                type = tx.ReferenceType.ToString(),
                id = tx.ReferenceId,
                lineId = tx.ReferenceLineId,
            },
            audit = new
            {
                schemaVersion = 1,
                emittedAt = DateTime.UtcNow,
                source = "AWBlazor",
            },
        };
    }

    private static Task<string?> LookupLocationCodeAsync(ApplicationDbContext db, int? id, CancellationToken ct)
        => id is null
            ? Task.FromResult<string?>(null)
            : db.InventoryLocations.Where(l => l.Id == id).Select(l => (string?)l.Code).FirstOrDefaultAsync(ct);

    private static Task<string?> LookupLotCodeAsync(ApplicationDbContext db, int? id, CancellationToken ct)
        => id is null
            ? Task.FromResult<string?>(null)
            : db.Lots.Where(l => l.Id == id).Select(l => (string?)l.LotCode).FirstOrDefaultAsync(ct);

    private static Task<string?> LookupSerialAsync(ApplicationDbContext db, int? id, CancellationToken ct)
        => id is null
            ? Task.FromResult<string?>(null)
            : db.SerialUnits.Where(s => s.Id == id).Select(s => (string?)s.SerialNumber).FirstOrDefaultAsync(ct);
}
