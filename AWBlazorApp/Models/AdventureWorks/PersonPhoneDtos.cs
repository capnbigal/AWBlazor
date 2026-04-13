using AWBlazorApp.Data.Entities.AdventureWorks;

namespace AWBlazorApp.Models.AdventureWorks;

public sealed record PersonPhoneDto(
    int BusinessEntityId, string PhoneNumber, int PhoneNumberTypeId, DateTime ModifiedDate);

public sealed record CreatePersonPhoneRequest
{
    public int BusinessEntityId { get; set; }
    public string? PhoneNumber { get; set; }
    public int PhoneNumberTypeId { get; set; }
}

/// <summary>
/// PersonPhone has no non-key columns to update — all three columns are part of the composite
/// PK. Touching this row updates ModifiedDate; to actually change the phone number itself the
/// caller must DELETE + POST a new row.
/// </summary>
public sealed record UpdatePersonPhoneRequest;

public sealed record PersonPhoneAuditLogDto(
    int Id, int BusinessEntityId, string PhoneNumber, int PhoneNumberTypeId,
    string Action, string? ChangedBy, DateTime ChangedDate, string? ChangeSummary,
    DateTime SourceModifiedDate);

public static class PersonPhoneMappings
{
    public static PersonPhoneDto ToDto(this PersonPhone e) => new(
        e.BusinessEntityId, e.PhoneNumber, e.PhoneNumberTypeId, e.ModifiedDate);

    public static PersonPhone ToEntity(this CreatePersonPhoneRequest r) => new()
    {
        BusinessEntityId = r.BusinessEntityId,
        PhoneNumber = (r.PhoneNumber ?? string.Empty).Trim(),
        PhoneNumberTypeId = r.PhoneNumberTypeId,
        ModifiedDate = DateTime.UtcNow,
    };

    public static void ApplyTo(this UpdatePersonPhoneRequest _, PersonPhone e)
    {
        e.ModifiedDate = DateTime.UtcNow;
    }

    public static PersonPhoneAuditLogDto ToDto(this PersonPhoneAuditLog a) => new(
        a.Id, a.BusinessEntityId, a.PhoneNumber, a.PhoneNumberTypeId,
        a.Action, a.ChangedBy, a.ChangedDate, a.ChangeSummary, a.SourceModifiedDate);
}
