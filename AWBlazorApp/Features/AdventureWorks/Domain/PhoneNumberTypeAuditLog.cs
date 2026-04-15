using System.ComponentModel.DataAnnotations;

namespace AWBlazorApp.Features.AdventureWorks.Domain;

/// <summary>Audit log for <see cref="PhoneNumberType"/>. EF-managed table <c>dbo.PhoneNumberTypeAuditLogs</c>.</summary>
public class PhoneNumberTypeAuditLog : AdventureWorksAuditLogBase
{
    public int PhoneNumberTypeId { get; set; }

    [MaxLength(50)] public string? Name { get; set; }
    public DateTime SourceModifiedDate { get; set; }
}
