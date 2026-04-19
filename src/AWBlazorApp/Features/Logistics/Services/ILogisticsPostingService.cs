namespace AWBlazorApp.Features.Logistics.Services;

/// <summary>
/// Single write path for the three Logistics posting flows. Each method flips a header
/// from <c>Approved</c> (or <c>Draft</c>) to <c>Posted</c>, posts one or more inventory
/// transactions via <c>IInventoryService</c>, and back-fills the transaction-id pointers
/// on each line so reviewers can walk from a receipt/shipment/transfer back to the ledger.
/// </summary>
public interface ILogisticsPostingService
{
    /// <summary>Post a <c>GoodsReceipt</c>. Enforces partial-receipt caps against the origin
    /// PO line when one is linked, auto-creates a <c>inv.Lot</c> row per line if the line's
    /// item tracks lots and no lot is specified, and writes one RECEIPT transaction per line.</summary>
    Task<PostingResult> PostReceiptAsync(int goodsReceiptId, string? userId, CancellationToken cancellationToken);

    /// <summary>Post a <c>Shipment</c>. Flips status from Draft/Picked/Packed to Shipped,
    /// stamps <c>ShippedAt</c>, and writes one SHIP transaction per line.</summary>
    Task<PostingResult> PostShipmentAsync(int shipmentId, string? userId, CancellationToken cancellationToken);

    /// <summary>Post a <c>StockTransfer</c>. Writes paired TRANSFER_OUT + TRANSFER_IN
    /// transactions per line sharing a single <c>CorrelationId</c> on the transfer header
    /// (so the ledger can pair the two legs).</summary>
    Task<PostingResult> PostTransferAsync(int stockTransferId, string? userId, CancellationToken cancellationToken);
}

public sealed record PostingResult(int HeaderId, int LinesPosted, List<long> TransactionIds);
