using AWBlazorApp.Features.Logistics.Domain;

namespace AWBlazorApp.Features.Logistics.Dtos;

public sealed record StockTransferDto(
    int Id, string TransferNumber, int FromLocationId, int ToLocationId,
    int? FromOrganizationId, int? ToOrganizationId, StockTransferStatus Status,
    Guid? CorrelationId, DateTime InitiatedAt, DateTime? CompletedAt,
    string? PostedByUserId, string? Notes, DateTime ModifiedDate);

public sealed record CreateStockTransferRequest
{
    public int FromLocationId { get; set; }
    public int ToLocationId { get; set; }
    public int? FromOrganizationId { get; set; }
    public int? ToOrganizationId { get; set; }
    public string? Notes { get; set; }
}

public sealed record UpdateStockTransferRequest
{
    public int? FromLocationId { get; set; }
    public int? ToLocationId { get; set; }
    public int? FromOrganizationId { get; set; }
    public int? ToOrganizationId { get; set; }
    public StockTransferStatus? Status { get; set; }
    public string? Notes { get; set; }
}

public sealed record StockTransferAuditLogDto(
    int Id, int StockTransferId, string Action, string? ChangedBy, DateTime ChangedDate, string? ChangeSummary,
    string? TransferNumber, int FromLocationId, int ToLocationId,
    int? FromOrganizationId, int? ToOrganizationId, StockTransferStatus Status,
    Guid? CorrelationId, DateTime InitiatedAt, DateTime? CompletedAt,
    string? PostedByUserId, string? Notes, DateTime SourceModifiedDate);

public sealed record StockTransferLineDto(
    int Id, int StockTransferId, int InventoryItemId, decimal Quantity, string UnitMeasureCode,
    int? LotId, int? SerialUnitId, long? FromTransactionId, long? ToTransactionId, DateTime ModifiedDate);

public sealed record CreateStockTransferLineRequest
{
    public int StockTransferId { get; set; }
    public int InventoryItemId { get; set; }
    public decimal Quantity { get; set; }
    public string? UnitMeasureCode { get; set; }
    public int? LotId { get; set; }
    public int? SerialUnitId { get; set; }
}

public sealed record UpdateStockTransferLineRequest
{
    public decimal? Quantity { get; set; }
    public int? LotId { get; set; }
    public int? SerialUnitId { get; set; }
}

public sealed record StockTransferLineAuditLogDto(
    int Id, int StockTransferLineId, string Action, string? ChangedBy, DateTime ChangedDate, string? ChangeSummary,
    int StockTransferId, int InventoryItemId, decimal Quantity, string? UnitMeasureCode,
    int? LotId, int? SerialUnitId, long? FromTransactionId, long? ToTransactionId, DateTime SourceModifiedDate);

public static class StockTransferMappings
{
    public static StockTransferDto ToDto(this StockTransfer e) => new(
        e.Id, e.TransferNumber, e.FromLocationId, e.ToLocationId,
        e.FromOrganizationId, e.ToOrganizationId, e.Status, e.CorrelationId,
        e.InitiatedAt, e.CompletedAt, e.PostedByUserId, e.Notes, e.ModifiedDate);

    public static StockTransfer ToEntity(this CreateStockTransferRequest r)
    {
        var now = DateTime.UtcNow;
        return new StockTransfer
        {
            TransferNumber = $"TRF-{now:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}",
            FromLocationId = r.FromLocationId,
            ToLocationId = r.ToLocationId,
            FromOrganizationId = r.FromOrganizationId,
            ToOrganizationId = r.ToOrganizationId,
            Status = StockTransferStatus.Draft,
            InitiatedAt = now,
            Notes = r.Notes?.Trim(),
            ModifiedDate = now,
        };
    }

    public static void ApplyTo(this UpdateStockTransferRequest r, StockTransfer e)
    {
        if (r.FromLocationId is not null) e.FromLocationId = r.FromLocationId.Value;
        if (r.ToLocationId is not null) e.ToLocationId = r.ToLocationId.Value;
        if (r.FromOrganizationId is not null) e.FromOrganizationId = r.FromOrganizationId;
        if (r.ToOrganizationId is not null) e.ToOrganizationId = r.ToOrganizationId;
        if (r.Status is not null) e.Status = r.Status.Value;
        if (r.Notes is not null) e.Notes = r.Notes.Trim();
        e.ModifiedDate = DateTime.UtcNow;
    }

    public static StockTransferAuditLogDto ToDto(this StockTransferAuditLog a) => new(
        a.Id, a.StockTransferId, a.Action, a.ChangedBy, a.ChangedDate, a.ChangeSummary,
        a.TransferNumber, a.FromLocationId, a.ToLocationId,
        a.FromOrganizationId, a.ToOrganizationId, a.Status, a.CorrelationId,
        a.InitiatedAt, a.CompletedAt, a.PostedByUserId, a.Notes, a.SourceModifiedDate);

    public static StockTransferLineDto ToDto(this StockTransferLine e) => new(
        e.Id, e.StockTransferId, e.InventoryItemId, e.Quantity, e.UnitMeasureCode,
        e.LotId, e.SerialUnitId, e.FromTransactionId, e.ToTransactionId, e.ModifiedDate);

    public static StockTransferLine ToEntity(this CreateStockTransferLineRequest r) => new()
    {
        StockTransferId = r.StockTransferId,
        InventoryItemId = r.InventoryItemId,
        Quantity = r.Quantity,
        UnitMeasureCode = (r.UnitMeasureCode ?? "EA").Trim().ToUpperInvariant(),
        LotId = r.LotId,
        SerialUnitId = r.SerialUnitId,
        ModifiedDate = DateTime.UtcNow,
    };

    public static void ApplyTo(this UpdateStockTransferLineRequest r, StockTransferLine e)
    {
        if (r.Quantity is not null) e.Quantity = r.Quantity.Value;
        if (r.LotId is not null) e.LotId = r.LotId;
        if (r.SerialUnitId is not null) e.SerialUnitId = r.SerialUnitId;
        e.ModifiedDate = DateTime.UtcNow;
    }

    public static StockTransferLineAuditLogDto ToDto(this StockTransferLineAuditLog a) => new(
        a.Id, a.StockTransferLineId, a.Action, a.ChangedBy, a.ChangedDate, a.ChangeSummary,
        a.StockTransferId, a.InventoryItemId, a.Quantity, a.UnitMeasureCode,
        a.LotId, a.SerialUnitId, a.FromTransactionId, a.ToTransactionId, a.SourceModifiedDate);
}
