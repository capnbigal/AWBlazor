using ElementaryApp.Data.Entities.AdventureWorks;

namespace ElementaryApp.Services.AdventureWorksAudit;

public static class SpecialOfferProductAuditService
{
    public const string ActionCreated = "Created";
    public const string ActionUpdated = "Updated";
    public const string ActionDeleted = "Deleted";

    public static SpecialOfferProductAuditLog RecordCreate(SpecialOfferProduct e, string? by)
        => BuildLog(e, ActionCreated, by, "Created");

    public static SpecialOfferProductAuditLog RecordUpdate(SpecialOfferProduct e, string? by)
        => BuildLog(e, ActionUpdated, by, "Touched");

    public static SpecialOfferProductAuditLog RecordDelete(SpecialOfferProduct e, string? by)
        => BuildLog(e, ActionDeleted, by, "Deleted");

    private static SpecialOfferProductAuditLog BuildLog(
        SpecialOfferProduct e, string action, string? by, string? summary)
        => new()
        {
            SpecialOfferId = e.SpecialOfferId,
            ProductId = e.ProductId,
            Action = action,
            ChangedBy = by,
            ChangedDate = DateTime.UtcNow,
            ChangeSummary = AuditDiffHelpers.Truncate(summary, 2048),
            RowGuid = e.RowGuid,
            SourceModifiedDate = e.ModifiedDate,
        };
}
