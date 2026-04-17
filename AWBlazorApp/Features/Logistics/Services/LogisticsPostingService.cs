using AWBlazorApp.Features.Inventory.Domain;
using AWBlazorApp.Features.Inventory.Services;
using AWBlazorApp.Features.Logistics.Domain;
using AWBlazorApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.Logistics.Services;

/// <inheritdoc />
public sealed class LogisticsPostingService(
    IDbContextFactory<ApplicationDbContext> dbFactory,
    IInventoryService inventoryService,
    ILogger<LogisticsPostingService> logger) : ILogisticsPostingService
{
    public async Task<PostingResult> PostReceiptAsync(int goodsReceiptId, string? userId, CancellationToken ct)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var header = await db.GoodsReceipts.FirstOrDefaultAsync(r => r.Id == goodsReceiptId, ct)
            ?? throw new InvalidOperationException($"GoodsReceipt {goodsReceiptId} not found.");
        EnsurePostable(header.Status, "GoodsReceipt");

        var lines = await db.GoodsReceiptLines.Where(l => l.GoodsReceiptId == goodsReceiptId).ToListAsync(ct);
        if (lines.Count == 0)
            throw new InvalidOperationException("Cannot post a receipt with no lines.");

        var txIds = new List<long>();
        foreach (var line in lines)
        {
            await EnforcePartialReceiptCapAsync(db, line, ct);
            var lotId = await EnsureLotForReceiptAsync(db, header, line, ct);

            var result = await inventoryService.PostTransactionAsync(new PostTransactionRequest(
                TypeCode: InventoryTransactionTypeCodes.Receipt,
                InventoryItemId: line.InventoryItemId,
                Quantity: line.Quantity,
                UnitMeasureCode: line.UnitMeasureCode,
                FromLocationId: null,
                ToLocationId: header.ReceivedLocationId,
                LotId: lotId,
                SerialUnitId: null,
                FromStatus: null,
                ToStatus: null,
                ReferenceType: TransactionReferenceKind.PurchaseOrder,
                ReferenceId: header.PurchaseOrderId,
                ReferenceLineId: line.PurchaseOrderDetailId,
                Notes: header.ReceiptNumber,
                CorrelationId: null,
                OccurredAt: header.ReceivedAt), userId, ct);

            line.LotId = lotId;
            line.PostedTransactionId = result.TransactionId;
            line.ModifiedDate = DateTime.UtcNow;
            txIds.Add(result.TransactionId);
        }

        header.Status = GoodsReceiptStatus.Posted;
        header.PostedAt = DateTime.UtcNow;
        header.PostedByUserId = userId;
        header.ModifiedDate = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Posted receipt {Number} ({Lines} lines, {Txs} transactions)",
            header.ReceiptNumber, lines.Count, txIds.Count);
        return new PostingResult(header.Id, lines.Count, txIds);
    }

    public async Task<PostingResult> PostShipmentAsync(int shipmentId, string? userId, CancellationToken ct)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var header = await db.Shipments.FirstOrDefaultAsync(s => s.Id == shipmentId, ct)
            ?? throw new InvalidOperationException($"Shipment {shipmentId} not found.");
        EnsureShipmentPostable(header.Status);

        var lines = await db.ShipmentLines.Where(l => l.ShipmentId == shipmentId).ToListAsync(ct);
        if (lines.Count == 0)
            throw new InvalidOperationException("Cannot ship with no lines.");

        var txIds = new List<long>();
        foreach (var line in lines)
        {
            var result = await inventoryService.PostTransactionAsync(new PostTransactionRequest(
                TypeCode: InventoryTransactionTypeCodes.Ship,
                InventoryItemId: line.InventoryItemId,
                Quantity: line.Quantity,
                UnitMeasureCode: line.UnitMeasureCode,
                FromLocationId: header.ShippedFromLocationId,
                ToLocationId: null,
                LotId: line.LotId,
                SerialUnitId: line.SerialUnitId,
                FromStatus: null,
                ToStatus: null,
                ReferenceType: TransactionReferenceKind.SalesOrder,
                ReferenceId: header.SalesOrderId,
                ReferenceLineId: line.SalesOrderDetailId,
                Notes: header.ShipmentNumber,
                CorrelationId: null,
                OccurredAt: header.ShippedAt ?? DateTime.UtcNow), userId, ct);

            line.PostedTransactionId = result.TransactionId;
            line.ModifiedDate = DateTime.UtcNow;
            txIds.Add(result.TransactionId);
        }

        header.Status = ShipmentStatus.Shipped;
        header.ShippedAt ??= DateTime.UtcNow;
        header.PostedAt = DateTime.UtcNow;
        header.PostedByUserId = userId;
        header.ModifiedDate = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Posted shipment {Number} ({Lines} lines, {Txs} transactions)",
            header.ShipmentNumber, lines.Count, txIds.Count);
        return new PostingResult(header.Id, lines.Count, txIds);
    }

    public async Task<PostingResult> PostTransferAsync(int stockTransferId, string? userId, CancellationToken ct)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var header = await db.StockTransfers.FirstOrDefaultAsync(t => t.Id == stockTransferId, ct)
            ?? throw new InvalidOperationException($"StockTransfer {stockTransferId} not found.");
        EnsureTransferPostable(header.Status);
        if (header.FromLocationId == header.ToLocationId)
            throw new InvalidOperationException("From and To locations must differ.");

        var lines = await db.StockTransferLines.Where(l => l.StockTransferId == stockTransferId).ToListAsync(ct);
        if (lines.Count == 0)
            throw new InvalidOperationException("Cannot post a transfer with no lines.");

        var correlationId = header.CorrelationId ?? Guid.NewGuid();
        var occurredAt = DateTime.UtcNow;
        var txIds = new List<long>();

        foreach (var line in lines)
        {
            var outResult = await inventoryService.PostTransactionAsync(new PostTransactionRequest(
                TypeCode: InventoryTransactionTypeCodes.TransferOut,
                InventoryItemId: line.InventoryItemId,
                Quantity: line.Quantity,
                UnitMeasureCode: line.UnitMeasureCode,
                FromLocationId: header.FromLocationId,
                ToLocationId: null,
                LotId: line.LotId,
                SerialUnitId: line.SerialUnitId,
                FromStatus: null,
                ToStatus: null,
                ReferenceType: TransactionReferenceKind.Transfer,
                ReferenceId: header.Id,
                ReferenceLineId: line.Id,
                Notes: header.TransferNumber,
                CorrelationId: correlationId,
                OccurredAt: occurredAt), userId, ct);

            var inResult = await inventoryService.PostTransactionAsync(new PostTransactionRequest(
                TypeCode: InventoryTransactionTypeCodes.TransferIn,
                InventoryItemId: line.InventoryItemId,
                Quantity: line.Quantity,
                UnitMeasureCode: line.UnitMeasureCode,
                FromLocationId: null,
                ToLocationId: header.ToLocationId,
                LotId: line.LotId,
                SerialUnitId: line.SerialUnitId,
                FromStatus: null,
                ToStatus: null,
                ReferenceType: TransactionReferenceKind.Transfer,
                ReferenceId: header.Id,
                ReferenceLineId: line.Id,
                Notes: header.TransferNumber,
                CorrelationId: correlationId,
                OccurredAt: occurredAt), userId, ct);

            line.FromTransactionId = outResult.TransactionId;
            line.ToTransactionId = inResult.TransactionId;
            line.ModifiedDate = DateTime.UtcNow;
            txIds.Add(outResult.TransactionId);
            txIds.Add(inResult.TransactionId);
        }

        header.Status = StockTransferStatus.Completed;
        header.CompletedAt = occurredAt;
        header.CorrelationId = correlationId;
        header.PostedByUserId = userId;
        header.ModifiedDate = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Posted transfer {Number} ({Lines} lines, {Txs} transactions)",
            header.TransferNumber, lines.Count, txIds.Count);
        return new PostingResult(header.Id, lines.Count, txIds);
    }

    private static void EnsurePostable(GoodsReceiptStatus status, string entity)
    {
        if (status is not (GoodsReceiptStatus.Draft or GoodsReceiptStatus.Approved))
            throw new InvalidOperationException($"{entity} status {status} is not postable.");
    }

    private static void EnsureShipmentPostable(ShipmentStatus status)
    {
        if (status is ShipmentStatus.Shipped or ShipmentStatus.Delivered or ShipmentStatus.Cancelled)
            throw new InvalidOperationException($"Shipment status {status} is not postable.");
    }

    private static void EnsureTransferPostable(StockTransferStatus status)
    {
        if (status is StockTransferStatus.Completed or StockTransferStatus.Cancelled)
            throw new InvalidOperationException($"StockTransfer status {status} is not postable.");
    }

    /// <summary>
    /// Rejects a receipt line whose quantity plus the running total of already-received
    /// quantity against the same PO line would exceed the PO line's OrderQty. Skipped when
    /// the line isn't linked to a PO (ad-hoc receipt).
    /// </summary>
    private static async Task EnforcePartialReceiptCapAsync(ApplicationDbContext db, GoodsReceiptLine line, CancellationToken ct)
    {
        if (line.PurchaseOrderDetailId is null) return;

        var orderQty = await db.Database
            .SqlQuery<decimal>($"SELECT CAST(OrderQty AS decimal(18,4)) AS Value FROM Purchasing.PurchaseOrderDetail WHERE PurchaseOrderDetailID = {line.PurchaseOrderDetailId}")
            .FirstOrDefaultAsync(ct);
        if (orderQty <= 0) return; // unknown — skip rather than block

        var alreadyReceived = await db.GoodsReceiptLines
            .Where(l => l.PurchaseOrderDetailId == line.PurchaseOrderDetailId && l.Id != line.Id && l.PostedTransactionId != null)
            .SumAsync(l => (decimal?)l.Quantity, ct) ?? 0m;

        if (alreadyReceived + line.Quantity > orderQty)
            throw new InvalidOperationException(
                $"Receipt line exceeds PO line: already received {alreadyReceived}, this line adds {line.Quantity}, PO ordered {orderQty}.");
    }

    /// <summary>
    /// If the inventory item tracks lots and the receipt line didn't pre-specify a lot id,
    /// auto-create an <c>inv.Lot</c> whose <c>LotCode</c> is the receipt number. Returns the
    /// lot id to stamp back onto the line (or the existing id if one was already set).
    /// </summary>
    private static async Task<int?> EnsureLotForReceiptAsync(
        ApplicationDbContext db, GoodsReceipt header, GoodsReceiptLine line, CancellationToken ct)
    {
        if (line.LotId is not null) return line.LotId;

        var item = await db.InventoryItems.AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == line.InventoryItemId, ct);
        if (item is null || !item.TracksLot) return null;

        var lotCode = $"{header.ReceiptNumber}-L{line.Id}";
        var lot = new Lot
        {
            InventoryItemId = line.InventoryItemId,
            LotCode = lotCode,
            ReceivedAt = header.ReceivedAt,
            VendorBusinessEntityId = header.VendorBusinessEntityId,
            Status = LotStatus.Available,
            ModifiedDate = DateTime.UtcNow,
        };
        db.Lots.Add(lot);
        await db.SaveChangesAsync(ct);
        return lot.Id;
    }
}
