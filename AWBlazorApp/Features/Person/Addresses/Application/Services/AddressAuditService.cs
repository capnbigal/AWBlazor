using AWBlazorApp.Shared.Audit;
using System.Text;
using AWBlazorApp.Features.Person.Addresses.Domain; using AWBlazorApp.Features.Person.AddressTypes.Domain; using AWBlazorApp.Features.Person.BusinessEntities.Domain; using AWBlazorApp.Features.Person.BusinessEntityAddresses.Domain; using AWBlazorApp.Features.Person.BusinessEntityContacts.Domain; using AWBlazorApp.Features.Person.ContactTypes.Domain; using AWBlazorApp.Features.Person.CountryRegions.Domain; using AWBlazorApp.Features.Person.EmailAddresses.Domain; using AWBlazorApp.Features.Person.Persons.Domain; using AWBlazorApp.Features.Person.PersonPhones.Domain; using AWBlazorApp.Features.Person.PhoneNumberTypes.Domain; using AWBlazorApp.Features.Person.StateProvinces.Domain; 

namespace AWBlazorApp.Features.Person.Addresses.Application.Services;

public static class AddressAuditService
{
    public const string ActionCreated = "Created";
    public const string ActionUpdated = "Updated";
    public const string ActionDeleted = "Deleted";

    public static Snapshot CaptureSnapshot(Address e) => new(e);

    public static AddressAuditLog RecordCreate(Address e, string? by)
        => BuildLog(e, ActionCreated, by, "Created");

    public static AddressAuditLog RecordUpdate(Snapshot before, Address after, string? by)
        => BuildLog(after, ActionUpdated, by, BuildDiffSummary(before, after));

    public static AddressAuditLog RecordDelete(Address e, string? by)
        => BuildLog(e, ActionDeleted, by, "Deleted");

    private static AddressAuditLog BuildLog(Address e, string action, string? by, string? summary)
        => new()
        {
            AddressId = e.Id,
            Action = action,
            ChangedBy = by,
            ChangedDate = DateTime.UtcNow,
            ChangeSummary = AuditDiffHelpers.Truncate(summary, 2048),
            AddressLine1 = e.AddressLine1,
            AddressLine2 = e.AddressLine2,
            City = e.City,
            StateProvinceId = e.StateProvinceId,
            PostalCode = e.PostalCode,
            RowGuid = e.RowGuid,
            SourceModifiedDate = e.ModifiedDate,
        };

    private static string BuildDiffSummary(Snapshot before, Address after)
    {
        var sb = new StringBuilder();
        AuditDiffHelpers.AppendIfChanged(sb, "AddressLine1", before.AddressLine1, after.AddressLine1);
        AuditDiffHelpers.AppendIfChanged(sb, "AddressLine2", before.AddressLine2, after.AddressLine2);
        AuditDiffHelpers.AppendIfChanged(sb, "City", before.City, after.City);
        AuditDiffHelpers.AppendIfChanged(sb, "StateProvinceId", before.StateProvinceId, after.StateProvinceId);
        AuditDiffHelpers.AppendIfChanged(sb, "PostalCode", before.PostalCode, after.PostalCode);
        return sb.Length == 0 ? "No changes" : sb.ToString();
    }

    public readonly record struct Snapshot(
        string AddressLine1, string? AddressLine2, string City, int StateProvinceId, string PostalCode)
    {
        public Snapshot(Address e) : this(e.AddressLine1, e.AddressLine2, e.City, e.StateProvinceId, e.PostalCode) { }
    }
}
