using System.Text;
using ElementaryApp.Data.Entities.AdventureWorks;

namespace ElementaryApp.Services.AdventureWorksAudit;

public static class VendorAuditService
{
    public const string ActionCreated = "Created";
    public const string ActionUpdated = "Updated";
    public const string ActionDeleted = "Deleted";

    public static Snapshot CaptureSnapshot(Vendor e) => new(e);

    public static VendorAuditLog RecordCreate(Vendor e, string? by)
        => BuildLog(e, ActionCreated, by, "Created");

    public static VendorAuditLog RecordUpdate(Snapshot before, Vendor after, string? by)
        => BuildLog(after, ActionUpdated, by, BuildDiffSummary(before, after));

    public static VendorAuditLog RecordDelete(Vendor e, string? by)
        => BuildLog(e, ActionDeleted, by, "Deleted");

    private static VendorAuditLog BuildLog(Vendor e, string action, string? by, string? summary)
        => new()
        {
            VendorId = e.Id,
            Action = action,
            ChangedBy = by,
            ChangedDate = DateTime.UtcNow,
            ChangeSummary = AuditDiffHelpers.Truncate(summary, 2048),
            AccountNumber = e.AccountNumber,
            Name = e.Name,
            CreditRating = e.CreditRating,
            PreferredVendorStatus = e.PreferredVendorStatus,
            ActiveFlag = e.ActiveFlag,
            PurchasingWebServiceUrl = e.PurchasingWebServiceUrl,
            SourceModifiedDate = e.ModifiedDate,
        };

    private static string BuildDiffSummary(Snapshot before, Vendor after)
    {
        var sb = new StringBuilder();
        AuditDiffHelpers.AppendIfChanged(sb, "AccountNumber", before.AccountNumber, after.AccountNumber);
        AuditDiffHelpers.AppendIfChanged(sb, "Name", before.Name, after.Name);
        AuditDiffHelpers.AppendIfChanged(sb, "CreditRating", before.CreditRating, after.CreditRating);
        AuditDiffHelpers.AppendIfChanged(sb, "PreferredVendorStatus", before.PreferredVendorStatus, after.PreferredVendorStatus);
        AuditDiffHelpers.AppendIfChanged(sb, "ActiveFlag", before.ActiveFlag, after.ActiveFlag);
        AuditDiffHelpers.AppendIfChanged(sb, "PurchasingWebServiceUrl", before.PurchasingWebServiceUrl, after.PurchasingWebServiceUrl);
        return sb.Length == 0 ? "No changes" : sb.ToString();
    }

    public readonly record struct Snapshot(
        string AccountNumber, string Name, byte CreditRating,
        bool PreferredVendorStatus, bool ActiveFlag, string? PurchasingWebServiceUrl)
    {
        public Snapshot(Vendor e) : this(
            e.AccountNumber, e.Name, e.CreditRating,
            e.PreferredVendorStatus, e.ActiveFlag, e.PurchasingWebServiceUrl)
        { }
    }
}
