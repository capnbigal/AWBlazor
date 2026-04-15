using System.Text;
using AWBlazorApp.Features.AdventureWorks.Domain;

namespace AWBlazorApp.Features.AdventureWorks.Audit;

public static class PurchaseOrderHeaderAuditService
{
    public const string ActionCreated = "Created";
    public const string ActionUpdated = "Updated";
    public const string ActionDeleted = "Deleted";

    public static Snapshot CaptureSnapshot(PurchaseOrderHeader e) => new(e);

    public static PurchaseOrderHeaderAuditLog RecordCreate(PurchaseOrderHeader e, string? by)
        => BuildLog(e, ActionCreated, by, "Created");

    public static PurchaseOrderHeaderAuditLog RecordUpdate(Snapshot before, PurchaseOrderHeader after, string? by)
        => BuildLog(after, ActionUpdated, by, BuildDiffSummary(before, after));

    public static PurchaseOrderHeaderAuditLog RecordDelete(PurchaseOrderHeader e, string? by)
        => BuildLog(e, ActionDeleted, by, "Deleted");

    private static PurchaseOrderHeaderAuditLog BuildLog(PurchaseOrderHeader e, string action, string? by, string? summary)
        => new()
        {
            PurchaseOrderId = e.Id,
            Action = action,
            ChangedBy = by,
            ChangedDate = DateTime.UtcNow,
            ChangeSummary = AuditDiffHelpers.Truncate(summary, 2048),
            RevisionNumber = e.RevisionNumber,
            Status = e.Status,
            EmployeeId = e.EmployeeId,
            VendorId = e.VendorId,
            ShipMethodId = e.ShipMethodId,
            OrderDate = e.OrderDate,
            ShipDate = e.ShipDate,
            SubTotal = e.SubTotal,
            TaxAmt = e.TaxAmt,
            Freight = e.Freight,
            TotalDue = e.TotalDue,
            SourceModifiedDate = e.ModifiedDate,
        };

    private static string BuildDiffSummary(Snapshot before, PurchaseOrderHeader after)
    {
        var sb = new StringBuilder();
        AuditDiffHelpers.AppendIfChanged(sb, "RevisionNumber", before.RevisionNumber, after.RevisionNumber);
        AuditDiffHelpers.AppendIfChanged(sb, "Status", before.Status, after.Status);
        AuditDiffHelpers.AppendIfChanged(sb, "EmployeeId", before.EmployeeId, after.EmployeeId);
        AuditDiffHelpers.AppendIfChanged(sb, "VendorId", before.VendorId, after.VendorId);
        AuditDiffHelpers.AppendIfChanged(sb, "ShipMethodId", before.ShipMethodId, after.ShipMethodId);
        AuditDiffHelpers.AppendIfChanged(sb, "OrderDate", before.OrderDate, after.OrderDate);
        AuditDiffHelpers.AppendIfChanged(sb, "ShipDate", before.ShipDate, after.ShipDate);
        AuditDiffHelpers.AppendIfChanged(sb, "SubTotal", before.SubTotal, after.SubTotal);
        AuditDiffHelpers.AppendIfChanged(sb, "TaxAmt", before.TaxAmt, after.TaxAmt);
        AuditDiffHelpers.AppendIfChanged(sb, "Freight", before.Freight, after.Freight);
        return sb.Length == 0 ? "No changes" : sb.ToString();
    }

    public readonly record struct Snapshot(
        byte RevisionNumber, byte Status, int EmployeeId, int VendorId, int ShipMethodId,
        DateTime OrderDate, DateTime? ShipDate, decimal SubTotal, decimal TaxAmt, decimal Freight)
    {
        public Snapshot(PurchaseOrderHeader e) : this(
            e.RevisionNumber, e.Status, e.EmployeeId, e.VendorId, e.ShipMethodId,
            e.OrderDate, e.ShipDate, e.SubTotal, e.TaxAmt, e.Freight)
        { }
    }
}
