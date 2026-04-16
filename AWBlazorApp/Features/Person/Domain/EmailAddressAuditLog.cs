using AWBlazorApp.Features.AdventureWorks.Domain;
using System.ComponentModel.DataAnnotations;

namespace AWBlazorApp.Features.Person.Domain;

/// <summary>Audit log for <see cref="EmailAddress"/>. EF-managed table <c>dbo.EmailAddressAuditLogs</c>. Carries both composite-key components.</summary>
public class EmailAddressAuditLog : AdventureWorksAuditLogBase
{
    public int BusinessEntityId { get; set; }
    public int EmailAddressId { get; set; }

    [MaxLength(50)] public string? EmailAddressValue { get; set; }
    public Guid RowGuid { get; set; }
    public DateTime SourceModifiedDate { get; set; }
}
