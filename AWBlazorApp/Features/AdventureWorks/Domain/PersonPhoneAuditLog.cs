using System.ComponentModel.DataAnnotations;

namespace AWBlazorApp.Features.AdventureWorks.Domain;

/// <summary>Audit log for <see cref="PersonPhone"/>. EF-managed table <c>dbo.PersonPhoneAuditLogs</c>. Carries all 3 composite-key components.</summary>
public class PersonPhoneAuditLog : AdventureWorksAuditLogBase
{
    public int BusinessEntityId { get; set; }
    [MaxLength(25)] public string PhoneNumber { get; set; } = string.Empty;
    public int PhoneNumberTypeId { get; set; }

    public DateTime SourceModifiedDate { get; set; }
}
