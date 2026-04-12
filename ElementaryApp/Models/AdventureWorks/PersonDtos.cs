using ElementaryApp.Data.Entities.AdventureWorks;

namespace ElementaryApp.Models.AdventureWorks;

public sealed record PersonDto(
    int Id, string PersonType, bool NameStyle, string? Title,
    string FirstName, string? MiddleName, string LastName, string? Suffix,
    int EmailPromotion, Guid RowGuid, DateTime ModifiedDate);

public sealed record CreatePersonRequest
{
    /// <summary>PK / FK to Person.BusinessEntity. NOT identity — caller must supply.</summary>
    public int Id { get; set; }
    public string? PersonType { get; set; }
    public bool NameStyle { get; set; }
    public string? Title { get; set; }
    public string? FirstName { get; set; }
    public string? MiddleName { get; set; }
    public string? LastName { get; set; }
    public string? Suffix { get; set; }
    public int EmailPromotion { get; set; }
}

public sealed record UpdatePersonRequest
{
    public string? PersonType { get; set; }
    public bool? NameStyle { get; set; }
    public string? Title { get; set; }
    public string? FirstName { get; set; }
    public string? MiddleName { get; set; }
    public string? LastName { get; set; }
    public string? Suffix { get; set; }
    public int? EmailPromotion { get; set; }
}

public sealed record PersonAuditLogDto(
    int Id, int PersonId, string Action, string? ChangedBy, DateTime ChangedDate,
    string? ChangeSummary, string? PersonType, bool NameStyle, string? Title,
    string? FirstName, string? MiddleName, string? LastName, string? Suffix,
    int EmailPromotion, Guid RowGuid, DateTime SourceModifiedDate);

public static class PersonMappings
{
    public static PersonDto ToDto(this Person e) => new(
        e.Id, e.PersonType, e.NameStyle, e.Title,
        e.FirstName, e.MiddleName, e.LastName, e.Suffix,
        e.EmailPromotion, e.RowGuid, e.ModifiedDate);

    public static Person ToEntity(this CreatePersonRequest r) => new()
    {
        Id = r.Id,
        PersonType = (r.PersonType ?? string.Empty).Trim(),
        NameStyle = r.NameStyle,
        Title = string.IsNullOrWhiteSpace(r.Title) ? null : r.Title.Trim(),
        FirstName = (r.FirstName ?? string.Empty).Trim(),
        MiddleName = string.IsNullOrWhiteSpace(r.MiddleName) ? null : r.MiddleName.Trim(),
        LastName = (r.LastName ?? string.Empty).Trim(),
        Suffix = string.IsNullOrWhiteSpace(r.Suffix) ? null : r.Suffix.Trim(),
        EmailPromotion = r.EmailPromotion,
        RowGuid = Guid.NewGuid(),
        ModifiedDate = DateTime.UtcNow,
    };

    public static void ApplyTo(this UpdatePersonRequest r, Person e)
    {
        if (r.PersonType is not null) e.PersonType = r.PersonType.Trim();
        if (r.NameStyle.HasValue) e.NameStyle = r.NameStyle.Value;
        if (r.Title is not null) e.Title = string.IsNullOrWhiteSpace(r.Title) ? null : r.Title.Trim();
        if (r.FirstName is not null) e.FirstName = r.FirstName.Trim();
        if (r.MiddleName is not null) e.MiddleName = string.IsNullOrWhiteSpace(r.MiddleName) ? null : r.MiddleName.Trim();
        if (r.LastName is not null) e.LastName = r.LastName.Trim();
        if (r.Suffix is not null) e.Suffix = string.IsNullOrWhiteSpace(r.Suffix) ? null : r.Suffix.Trim();
        if (r.EmailPromotion.HasValue) e.EmailPromotion = r.EmailPromotion.Value;
        e.ModifiedDate = DateTime.UtcNow;
    }

    public static PersonAuditLogDto ToDto(this PersonAuditLog a) => new(
        a.Id, a.PersonId, a.Action, a.ChangedBy, a.ChangedDate, a.ChangeSummary,
        a.PersonType, a.NameStyle, a.Title, a.FirstName, a.MiddleName, a.LastName, a.Suffix,
        a.EmailPromotion, a.RowGuid, a.SourceModifiedDate);
}
