namespace AWBlazorApp.Data.Entities.AdventureWorks;

/// <summary>Audit log for <see cref="Illustration"/>. EF-managed table <c>dbo.IllustrationAuditLogs</c>.</summary>
public class IllustrationAuditLog : AdventureWorksAuditLogBase
{
    public int IllustrationId { get; set; }

    public DateTime SourceModifiedDate { get; set; }
}
