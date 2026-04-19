using AWBlazorApp.Shared.Audit;
using System.Text;
using AWBlazorApp.Features.Sales.Domain;

namespace AWBlazorApp.Features.Sales.Audit;

public static class SalesOrderHeaderAuditService
{
    public const string ActionCreated = "Created";
    public const string ActionUpdated = "Updated";
    public const string ActionDeleted = "Deleted";

    public static Snapshot CaptureSnapshot(SalesOrderHeader e) => new(e);

    public static SalesOrderHeaderAuditLog RecordCreate(SalesOrderHeader e, string? by)
        => BuildLog(e, ActionCreated, by, "Created");

    public static SalesOrderHeaderAuditLog RecordUpdate(Snapshot before, SalesOrderHeader after, string? by)
        => BuildLog(after, ActionUpdated, by, BuildDiffSummary(before, after));

    public static SalesOrderHeaderAuditLog RecordDelete(SalesOrderHeader e, string? by)
        => BuildLog(e, ActionDeleted, by, "Deleted");

    private static SalesOrderHeaderAuditLog BuildLog(SalesOrderHeader e, string action, string? by, string? summary)
        => new()
        {
            SalesOrderId = e.Id,
            Action = action,
            ChangedBy = by,
            ChangedDate = DateTime.UtcNow,
            ChangeSummary = AuditDiffHelpers.Truncate(summary, 2048),
            RevisionNumber = e.RevisionNumber,
            OrderDate = e.OrderDate,
            DueDate = e.DueDate,
            ShipDate = e.ShipDate,
            Status = e.Status,
            OnlineOrderFlag = e.OnlineOrderFlag,
            SalesOrderNumber = e.SalesOrderNumber,
            PurchaseOrderNumber = e.PurchaseOrderNumber,
            AccountNumber = e.AccountNumber,
            CustomerId = e.CustomerId,
            SalesPersonId = e.SalesPersonId,
            TerritoryId = e.TerritoryId,
            BillToAddressId = e.BillToAddressId,
            ShipToAddressId = e.ShipToAddressId,
            ShipMethodId = e.ShipMethodId,
            CreditCardId = e.CreditCardId,
            CreditCardApprovalCode = e.CreditCardApprovalCode,
            CurrencyRateId = e.CurrencyRateId,
            SubTotal = e.SubTotal,
            TaxAmt = e.TaxAmt,
            Freight = e.Freight,
            TotalDue = e.TotalDue,
            Comment = e.Comment,
            RowGuid = e.RowGuid,
            SourceModifiedDate = e.ModifiedDate,
        };

    private static string BuildDiffSummary(Snapshot before, SalesOrderHeader after)
    {
        var sb = new StringBuilder();
        AuditDiffHelpers.AppendIfChanged(sb, "DueDate", before.DueDate, after.DueDate);
        AuditDiffHelpers.AppendIfChanged(sb, "ShipDate", before.ShipDate, after.ShipDate);
        AuditDiffHelpers.AppendIfChanged(sb, "Status", before.Status, after.Status);
        AuditDiffHelpers.AppendIfChanged(sb, "PurchaseOrderNumber", before.PurchaseOrderNumber, after.PurchaseOrderNumber);
        AuditDiffHelpers.AppendIfChanged(sb, "AccountNumber", before.AccountNumber, after.AccountNumber);
        AuditDiffHelpers.AppendIfChanged(sb, "SalesPersonId", before.SalesPersonId, after.SalesPersonId);
        AuditDiffHelpers.AppendIfChanged(sb, "TerritoryId", before.TerritoryId, after.TerritoryId);
        AuditDiffHelpers.AppendIfChanged(sb, "ShipMethodId", before.ShipMethodId, after.ShipMethodId);
        AuditDiffHelpers.AppendIfChanged(sb, "CreditCardId", before.CreditCardId, after.CreditCardId);
        AuditDiffHelpers.AppendIfChanged(sb, "SubTotal", before.SubTotal, after.SubTotal);
        AuditDiffHelpers.AppendIfChanged(sb, "TaxAmt", before.TaxAmt, after.TaxAmt);
        AuditDiffHelpers.AppendIfChanged(sb, "Freight", before.Freight, after.Freight);
        AuditDiffHelpers.AppendIfChanged(sb, "Comment", before.Comment, after.Comment);
        return sb.Length == 0 ? "No changes" : sb.ToString();
    }

    public readonly record struct Snapshot(
        DateTime DueDate, DateTime? ShipDate, byte Status,
        string? PurchaseOrderNumber, string? AccountNumber,
        int? SalesPersonId, int? TerritoryId, int ShipMethodId,
        int? CreditCardId, decimal SubTotal, decimal TaxAmt, decimal Freight,
        string? Comment)
    {
        public Snapshot(SalesOrderHeader e) : this(
            e.DueDate, e.ShipDate, e.Status,
            e.PurchaseOrderNumber, e.AccountNumber,
            e.SalesPersonId, e.TerritoryId, e.ShipMethodId,
            e.CreditCardId, e.SubTotal, e.TaxAmt, e.Freight,
            e.Comment)
        { }
    }
}
