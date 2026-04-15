using AWBlazorApp.Features.AdventureWorks.Domain;

namespace AWBlazorApp.Features.AdventureWorks.Audit;

public static class BusinessEntityAuditService
{
    public const string ActionCreated = "Created";
    public const string ActionUpdated = "Updated";
    public const string ActionDeleted = "Deleted";

    public static BusinessEntityAuditLog RecordCreate(BusinessEntity e, string? by)
        => BuildLog(e, ActionCreated, by, "Created");

    public static BusinessEntityAuditLog RecordUpdate(BusinessEntity e, string? by)
        => BuildLog(e, ActionUpdated, by, "Touched");

    public static BusinessEntityAuditLog RecordDelete(BusinessEntity e, string? by)
        => BuildLog(e, ActionDeleted, by, "Deleted");

    private static BusinessEntityAuditLog BuildLog(BusinessEntity e, string action, string? by, string? summary)
        => new()
        {
            BusinessEntityId = e.Id,
            Action = action,
            ChangedBy = by,
            ChangedDate = DateTime.UtcNow,
            ChangeSummary = AuditDiffHelpers.Truncate(summary, 2048),
            RowGuid = e.RowGuid,
            SourceModifiedDate = e.ModifiedDate,
        };
}
