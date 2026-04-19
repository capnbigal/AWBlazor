using AWBlazorApp.Features.Person.Domain;

namespace AWBlazorApp.Features.Person.Dtos;

public sealed record PhoneNumberTypeDto(int Id, string Name, DateTime ModifiedDate);

public sealed record CreatePhoneNumberTypeRequest
{
    public string? Name { get; set; }
}

public sealed record UpdatePhoneNumberTypeRequest
{
    public string? Name { get; set; }
}

public sealed record PhoneNumberTypeAuditLogDto(
    int Id, int PhoneNumberTypeId, string Action, string? ChangedBy, DateTime ChangedDate,
    string? ChangeSummary, string? Name, DateTime SourceModifiedDate);

public static class PhoneNumberTypeMappings
{
    public static PhoneNumberTypeDto ToDto(this PhoneNumberType e) => new(e.Id, e.Name, e.ModifiedDate);

    public static PhoneNumberType ToEntity(this CreatePhoneNumberTypeRequest r) => new()
    {
        Name = (r.Name ?? string.Empty).Trim(),
        ModifiedDate = DateTime.UtcNow,
    };

    public static void ApplyTo(this UpdatePhoneNumberTypeRequest r, PhoneNumberType e)
    {
        if (r.Name is not null) e.Name = r.Name.Trim();
        e.ModifiedDate = DateTime.UtcNow;
    }

    public static PhoneNumberTypeAuditLogDto ToDto(this PhoneNumberTypeAuditLog a) => new(
        a.Id, a.PhoneNumberTypeId, a.Action, a.ChangedBy, a.ChangedDate, a.ChangeSummary,
        a.Name, a.SourceModifiedDate);
}
