using AWBlazorApp.Shared.Domain;
namespace AWBlazorApp.Features.Sales.Domain;

/// <summary>Audit log for <see cref="PersonCreditCard"/>. EF-managed table <c>dbo.PersonCreditCardAuditLogs</c>. Pure junction — audit rows only carry the composite key plus timestamp.</summary>
public class PersonCreditCardAuditLog : AdventureWorksAuditLogBase
{
    public int BusinessEntityId { get; set; }
    public int CreditCardId { get; set; }

    public DateTime SourceModifiedDate { get; set; }
}
