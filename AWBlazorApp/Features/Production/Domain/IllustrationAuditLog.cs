using AWBlazorApp.Features.AdventureWorks.Domain;
namespace AWBlazorApp.Features.Production.Domain;

/// <summary>Audit log for <see cref="Illustration"/>. EF-managed table <c>dbo.IllustrationAuditLogs</c>.</summary>
public class IllustrationAuditLog : AdventureWorksAuditLogBase
{
    public int IllustrationId { get; set; }

    public DateTime SourceModifiedDate { get; set; }
}
