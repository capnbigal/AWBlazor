using AWBlazorApp.Shared.Audit;
using System.Text;
using AWBlazorApp.Features.Purchasing.Domain;

namespace AWBlazorApp.Features.Purchasing.Audit;

public static class ProductVendorAuditService
{
    public const string ActionCreated = "Created";
    public const string ActionUpdated = "Updated";
    public const string ActionDeleted = "Deleted";

    public static Snapshot CaptureSnapshot(ProductVendor e) => new(e);

    public static ProductVendorAuditLog RecordCreate(ProductVendor e, string? by)
        => BuildLog(e, ActionCreated, by, "Created");

    public static ProductVendorAuditLog RecordUpdate(Snapshot before, ProductVendor after, string? by)
        => BuildLog(after, ActionUpdated, by, BuildDiffSummary(before, after));

    public static ProductVendorAuditLog RecordDelete(ProductVendor e, string? by)
        => BuildLog(e, ActionDeleted, by, "Deleted");

    private static ProductVendorAuditLog BuildLog(ProductVendor e, string action, string? by, string? summary)
        => new()
        {
            ProductId = e.ProductId,
            BusinessEntityId = e.BusinessEntityId,
            Action = action,
            ChangedBy = by,
            ChangedDate = DateTime.UtcNow,
            ChangeSummary = AuditDiffHelpers.Truncate(summary, 2048),
            AverageLeadTime = e.AverageLeadTime,
            StandardPrice = e.StandardPrice,
            LastReceiptCost = e.LastReceiptCost,
            LastReceiptDate = e.LastReceiptDate,
            MinOrderQty = e.MinOrderQty,
            MaxOrderQty = e.MaxOrderQty,
            OnOrderQty = e.OnOrderQty,
            UnitMeasureCode = e.UnitMeasureCode,
            SourceModifiedDate = e.ModifiedDate,
        };

    private static string BuildDiffSummary(Snapshot before, ProductVendor after)
    {
        var sb = new StringBuilder();
        AuditDiffHelpers.AppendIfChanged(sb, "AverageLeadTime", before.AverageLeadTime, after.AverageLeadTime);
        AuditDiffHelpers.AppendIfChanged(sb, "StandardPrice", before.StandardPrice, after.StandardPrice);
        AuditDiffHelpers.AppendIfChanged(sb, "LastReceiptCost", before.LastReceiptCost, after.LastReceiptCost);
        AuditDiffHelpers.AppendIfChanged(sb, "LastReceiptDate", before.LastReceiptDate, after.LastReceiptDate);
        AuditDiffHelpers.AppendIfChanged(sb, "MinOrderQty", before.MinOrderQty, after.MinOrderQty);
        AuditDiffHelpers.AppendIfChanged(sb, "MaxOrderQty", before.MaxOrderQty, after.MaxOrderQty);
        AuditDiffHelpers.AppendIfChanged(sb, "OnOrderQty", before.OnOrderQty, after.OnOrderQty);
        AuditDiffHelpers.AppendIfChanged(sb, "UnitMeasureCode", before.UnitMeasureCode, after.UnitMeasureCode);
        return sb.Length == 0 ? "No changes" : sb.ToString();
    }

    public readonly record struct Snapshot(
        int AverageLeadTime, decimal StandardPrice, decimal? LastReceiptCost,
        DateTime? LastReceiptDate, int MinOrderQty, int MaxOrderQty,
        int? OnOrderQty, string UnitMeasureCode)
    {
        public Snapshot(ProductVendor e) : this(
            e.AverageLeadTime, e.StandardPrice, e.LastReceiptCost,
            e.LastReceiptDate, e.MinOrderQty, e.MaxOrderQty,
            e.OnOrderQty, e.UnitMeasureCode)
        { }
    }
}
