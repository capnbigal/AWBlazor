using AWBlazorApp.Shared.Domain;
using System.ComponentModel.DataAnnotations;

namespace AWBlazorApp.Features.Person.Domain;

/// <summary>Audit log for <see cref="ContactType"/>. EF-managed table <c>dbo.ContactTypeAuditLogs</c>.</summary>
public class ContactTypeAuditLog : AdventureWorksAuditLogBase
{
    public int ContactTypeId { get; set; }

    [MaxLength(50)] public string? Name { get; set; }
    public DateTime SourceModifiedDate { get; set; }
}
