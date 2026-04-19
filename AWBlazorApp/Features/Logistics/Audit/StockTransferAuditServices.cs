using AWBlazorApp.Shared.Audit;
using AWBlazorApp.Features.Logistics.Domain;
using System.Text;

namespace AWBlazorApp.Features.Logistics.Audit;

public static class StockTransferAuditService
{
    public const string ActionCreated = "Created";
    public const string ActionUpdated = "Updated";
    public const string ActionDeleted = "Deleted";

    public static Snapshot CaptureSnapshot(StockTransfer e) => new(e);

    public static StockTransferAuditLog RecordCreate(StockTransfer e, string? by) => Build(e, ActionCreated, by, "Created");
    public static StockTransferAuditLog RecordUpdate(Snapshot before, StockTransfer after, string? by)
        => Build(after, ActionUpdated, by, Diff(before, after));
    public static StockTransferAuditLog RecordDelete(StockTransfer e, string? by) => Build(e, ActionDeleted, by, "Deleted");

    private static StockTransferAuditLog Build(StockTransfer e, string action, string? by, string? summary) => new()
    {
        StockTransferId = e.Id,
        Action = action,
        ChangedBy = by,
        ChangedDate = DateTime.UtcNow,
        ChangeSummary = AuditDiffHelpers.Truncate(summary, 2048),
        TransferNumber = e.TransferNumber,
        FromLocationId = e.FromLocationId,
        ToLocationId = e.ToLocationId,
        FromOrganizationId = e.FromOrganizationId,
        ToOrganizationId = e.ToOrganizationId,
        Status = e.Status,
        CorrelationId = e.CorrelationId,
        InitiatedAt = e.InitiatedAt,
        CompletedAt = e.CompletedAt,
        PostedByUserId = e.PostedByUserId,
        Notes = e.Notes,
        SourceModifiedDate = e.ModifiedDate,
    };

    private static string Diff(Snapshot b, StockTransfer a)
    {
        var sb = new StringBuilder();
        AuditDiffHelpers.AppendIfChanged(sb, "FromLocationId", b.FromLocationId, a.FromLocationId);
        AuditDiffHelpers.AppendIfChanged(sb, "ToLocationId", b.ToLocationId, a.ToLocationId);
        AuditDiffHelpers.AppendIfChanged(sb, "FromOrganizationId", b.FromOrganizationId, a.FromOrganizationId);
        AuditDiffHelpers.AppendIfChanged(sb, "ToOrganizationId", b.ToOrganizationId, a.ToOrganizationId);
        AuditDiffHelpers.AppendIfChanged(sb, "Status", b.Status, a.Status);
        AuditDiffHelpers.AppendIfChanged(sb, "CompletedAt", b.CompletedAt, a.CompletedAt);
        AuditDiffHelpers.AppendIfChanged(sb, "Notes", b.Notes, a.Notes);
        return sb.Length == 0 ? "No changes" : sb.ToString();
    }

    public readonly record struct Snapshot(
        int FromLocationId, int ToLocationId, int? FromOrganizationId, int? ToOrganizationId,
        StockTransferStatus Status, DateTime? CompletedAt, string? Notes)
    {
        public Snapshot(StockTransfer e) : this(
            e.FromLocationId, e.ToLocationId, e.FromOrganizationId, e.ToOrganizationId,
            e.Status, e.CompletedAt, e.Notes) { }
    }
}

public static class StockTransferLineAuditService
{
    public const string ActionCreated = "Created";
    public const string ActionUpdated = "Updated";
    public const string ActionDeleted = "Deleted";

    public static Snapshot CaptureSnapshot(StockTransferLine e) => new(e);

    public static StockTransferLineAuditLog RecordCreate(StockTransferLine e, string? by) => Build(e, ActionCreated, by, "Created");
    public static StockTransferLineAuditLog RecordUpdate(Snapshot before, StockTransferLine after, string? by)
        => Build(after, ActionUpdated, by, Diff(before, after));
    public static StockTransferLineAuditLog RecordDelete(StockTransferLine e, string? by) => Build(e, ActionDeleted, by, "Deleted");

    private static StockTransferLineAuditLog Build(StockTransferLine e, string action, string? by, string? summary) => new()
    {
        StockTransferLineId = e.Id,
        Action = action,
        ChangedBy = by,
        ChangedDate = DateTime.UtcNow,
        ChangeSummary = AuditDiffHelpers.Truncate(summary, 2048),
        StockTransferId = e.StockTransferId,
        InventoryItemId = e.InventoryItemId,
        Quantity = e.Quantity,
        UnitMeasureCode = e.UnitMeasureCode,
        LotId = e.LotId,
        SerialUnitId = e.SerialUnitId,
        FromTransactionId = e.FromTransactionId,
        ToTransactionId = e.ToTransactionId,
        SourceModifiedDate = e.ModifiedDate,
    };

    private static string Diff(Snapshot b, StockTransferLine a)
    {
        var sb = new StringBuilder();
        AuditDiffHelpers.AppendIfChanged(sb, "Quantity", b.Quantity, a.Quantity);
        AuditDiffHelpers.AppendIfChanged(sb, "LotId", b.LotId, a.LotId);
        AuditDiffHelpers.AppendIfChanged(sb, "SerialUnitId", b.SerialUnitId, a.SerialUnitId);
        return sb.Length == 0 ? "No changes" : sb.ToString();
    }

    public readonly record struct Snapshot(decimal Quantity, int? LotId, int? SerialUnitId)
    {
        public Snapshot(StockTransferLine e) : this(e.Quantity, e.LotId, e.SerialUnitId) { }
    }
}
