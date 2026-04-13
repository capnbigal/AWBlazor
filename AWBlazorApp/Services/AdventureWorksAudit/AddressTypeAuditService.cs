using System.Text;
using AWBlazorApp.Data.Entities.AdventureWorks;

namespace AWBlazorApp.Services.AdventureWorksAudit;

/// <summary>
/// Builds <see cref="AddressTypeAuditLog"/> rows for create/update/delete of <see cref="AddressType"/>.
/// Callers add the returned row to their existing <c>ApplicationDbContext</c> and save in the
/// same transaction as the underlying CRUD operation, matching the ToolSlots audit pattern.
/// </summary>
public static class AddressTypeAuditService
{
    public const string ActionCreated = "Created";
    public const string ActionUpdated = "Updated";
    public const string ActionDeleted = "Deleted";

    /// <summary>Capture the pre-update scalar state of an entity before mutating it.</summary>
    public static Snapshot CaptureSnapshot(AddressType e) => new(e);

    public static AddressTypeAuditLog RecordCreate(AddressType e, string? by)
        => BuildLog(e, ActionCreated, by, "Created");

    public static AddressTypeAuditLog RecordUpdate(Snapshot before, AddressType after, string? by)
        => BuildLog(after, ActionUpdated, by, BuildDiffSummary(before, after));

    public static AddressTypeAuditLog RecordDelete(AddressType e, string? by)
        => BuildLog(e, ActionDeleted, by, "Deleted");

    private static AddressTypeAuditLog BuildLog(AddressType e, string action, string? by, string? summary)
        => new()
        {
            AddressTypeId = e.Id,
            Action = action,
            ChangedBy = by,
            ChangedDate = DateTime.UtcNow,
            ChangeSummary = AuditDiffHelpers.Truncate(summary, 2048),
            Name = e.Name,
            RowGuid = e.RowGuid,
            SourceModifiedDate = e.ModifiedDate,
        };

    private static string BuildDiffSummary(Snapshot before, AddressType after)
    {
        var sb = new StringBuilder();
        AuditDiffHelpers.AppendIfChanged(sb, "Name", before.Name, after.Name);
        return sb.Length == 0 ? "No changes" : sb.ToString();
    }

    public readonly record struct Snapshot(string Name)
    {
        public Snapshot(AddressType e) : this(e.Name) { }
    }
}
