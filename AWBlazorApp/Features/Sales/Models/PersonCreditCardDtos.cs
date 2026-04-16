using AWBlazorApp.Features.Sales.Domain;

namespace AWBlazorApp.Features.Sales.Models;

public sealed record PersonCreditCardDto(
    int BusinessEntityId, int CreditCardId, DateTime ModifiedDate);

public sealed record CreatePersonCreditCardRequest
{
    public int BusinessEntityId { get; set; }
    public int CreditCardId { get; set; }
}

/// <summary>
/// Pure junction table — there are no non-key columns to update beyond ModifiedDate, so
/// PATCH effectively just touches the row's modified-date stamp.
/// </summary>
public sealed record UpdatePersonCreditCardRequest
{
    // Reserved for future expansion. Touching this row updates ModifiedDate.
}

public sealed record PersonCreditCardAuditLogDto(
    int Id, int BusinessEntityId, int CreditCardId, string Action,
    string? ChangedBy, DateTime ChangedDate, string? ChangeSummary, DateTime SourceModifiedDate);

public static class PersonCreditCardMappings
{
    public static PersonCreditCardDto ToDto(this PersonCreditCard e) => new(
        e.BusinessEntityId, e.CreditCardId, e.ModifiedDate);

    public static PersonCreditCard ToEntity(this CreatePersonCreditCardRequest r) => new()
    {
        BusinessEntityId = r.BusinessEntityId,
        CreditCardId = r.CreditCardId,
        ModifiedDate = DateTime.UtcNow,
    };

    public static void ApplyTo(this UpdatePersonCreditCardRequest _, PersonCreditCard e)
    {
        e.ModifiedDate = DateTime.UtcNow;
    }

    public static PersonCreditCardAuditLogDto ToDto(this PersonCreditCardAuditLog a) => new(
        a.Id, a.BusinessEntityId, a.CreditCardId, a.Action, a.ChangedBy, a.ChangedDate,
        a.ChangeSummary, a.SourceModifiedDate);
}
