using AWBlazorApp.Shared.Audit;
using System.Text;
using AWBlazorApp.Features.Person.Addresses.Domain; using AWBlazorApp.Features.Person.AddressTypes.Domain; using AWBlazorApp.Features.Person.BusinessEntities.Domain; using AWBlazorApp.Features.Person.BusinessEntityAddresses.Domain; using AWBlazorApp.Features.Person.BusinessEntityContacts.Domain; using AWBlazorApp.Features.Person.ContactTypes.Domain; using AWBlazorApp.Features.Person.CountryRegions.Domain; using AWBlazorApp.Features.Person.EmailAddresses.Domain; using AWBlazorApp.Features.Person.Persons.Domain; using AWBlazorApp.Features.Person.PersonPhones.Domain; using AWBlazorApp.Features.Person.PhoneNumberTypes.Domain; using AWBlazorApp.Features.Person.StateProvinces.Domain; 

namespace AWBlazorApp.Features.Person.CountryRegions.Application.Services;

public static class CountryRegionAuditService
{
    public const string ActionCreated = "Created";
    public const string ActionUpdated = "Updated";
    public const string ActionDeleted = "Deleted";

    public static Snapshot CaptureSnapshot(CountryRegion e) => new(e);

    public static CountryRegionAuditLog RecordCreate(CountryRegion e, string? by)
        => BuildLog(e, ActionCreated, by, "Created");

    public static CountryRegionAuditLog RecordUpdate(Snapshot before, CountryRegion after, string? by)
        => BuildLog(after, ActionUpdated, by, BuildDiffSummary(before, after));

    public static CountryRegionAuditLog RecordDelete(CountryRegion e, string? by)
        => BuildLog(e, ActionDeleted, by, "Deleted");

    private static CountryRegionAuditLog BuildLog(CountryRegion e, string action, string? by, string? summary)
        => new()
        {
            CountryRegionCode = e.CountryRegionCode,
            Action = action,
            ChangedBy = by,
            ChangedDate = DateTime.UtcNow,
            ChangeSummary = AuditDiffHelpers.Truncate(summary, 2048),
            Name = e.Name,
            SourceModifiedDate = e.ModifiedDate,
        };

    private static string BuildDiffSummary(Snapshot before, CountryRegion after)
    {
        var sb = new StringBuilder();
        AuditDiffHelpers.AppendIfChanged(sb, "Name", before.Name, after.Name);
        return sb.Length == 0 ? "No changes" : sb.ToString();
    }

    public readonly record struct Snapshot(string Name)
    {
        public Snapshot(CountryRegion e) : this(e.Name) { }
    }
}
