using AWBlazorApp.Features.Person.Domain;

namespace AWBlazorApp.Features.Person.Dtos;

public sealed record BusinessEntityContactDto(
    int BusinessEntityId, int PersonId, int ContactTypeId, Guid RowGuid, DateTime ModifiedDate);

public sealed record CreateBusinessEntityContactRequest
{
    public int BusinessEntityId { get; set; }
    public int PersonId { get; set; }
    public int ContactTypeId { get; set; }
}

/// <summary>
/// Pure junction — there are no non-key columns to update beyond ModifiedDate.
/// </summary>
public sealed record UpdateBusinessEntityContactRequest;

public sealed record BusinessEntityContactAuditLogDto(
    int Id, int BusinessEntityId, int PersonId, int ContactTypeId,
    string Action, string? ChangedBy, DateTime ChangedDate, string? ChangeSummary,
    Guid RowGuid, DateTime SourceModifiedDate);

public static class BusinessEntityContactMappings
{
    public static BusinessEntityContactDto ToDto(this BusinessEntityContact e) => new(
        e.BusinessEntityId, e.PersonId, e.ContactTypeId, e.RowGuid, e.ModifiedDate);

    public static BusinessEntityContact ToEntity(this CreateBusinessEntityContactRequest r) => new()
    {
        BusinessEntityId = r.BusinessEntityId,
        PersonId = r.PersonId,
        ContactTypeId = r.ContactTypeId,
        RowGuid = Guid.NewGuid(),
        ModifiedDate = DateTime.UtcNow,
    };

    public static void ApplyTo(this UpdateBusinessEntityContactRequest _, BusinessEntityContact e)
    {
        e.ModifiedDate = DateTime.UtcNow;
    }

    public static BusinessEntityContactAuditLogDto ToDto(this BusinessEntityContactAuditLog a) => new(
        a.Id, a.BusinessEntityId, a.PersonId, a.ContactTypeId,
        a.Action, a.ChangedBy, a.ChangedDate, a.ChangeSummary,
        a.RowGuid, a.SourceModifiedDate);
}
