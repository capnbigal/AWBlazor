using ElementaryApp.Data.Entities.AdventureWorks;

namespace ElementaryApp.Services.AdventureWorksAudit;

public static class SalesOrderHeaderSalesReasonAuditService
{
    public const string ActionCreated = "Created";
    public const string ActionUpdated = "Updated";
    public const string ActionDeleted = "Deleted";

    public static SalesOrderHeaderSalesReasonAuditLog RecordCreate(SalesOrderHeaderSalesReason e, string? by)
        => BuildLog(e, ActionCreated, by, "Created");

    public static SalesOrderHeaderSalesReasonAuditLog RecordUpdate(SalesOrderHeaderSalesReason e, string? by)
        => BuildLog(e, ActionUpdated, by, "Touched");

    public static SalesOrderHeaderSalesReasonAuditLog RecordDelete(SalesOrderHeaderSalesReason e, string? by)
        => BuildLog(e, ActionDeleted, by, "Deleted");

    private static SalesOrderHeaderSalesReasonAuditLog BuildLog(
        SalesOrderHeaderSalesReason e, string action, string? by, string? summary)
        => new()
        {
            SalesOrderId = e.SalesOrderId,
            SalesReasonId = e.SalesReasonId,
            Action = action,
            ChangedBy = by,
            ChangedDate = DateTime.UtcNow,
            ChangeSummary = AuditDiffHelpers.Truncate(summary, 2048),
            SourceModifiedDate = e.ModifiedDate,
        };
}
