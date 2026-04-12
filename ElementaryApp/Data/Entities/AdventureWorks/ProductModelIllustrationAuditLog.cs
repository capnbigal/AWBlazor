namespace ElementaryApp.Data.Entities.AdventureWorks;

/// <summary>Audit log for <see cref="ProductModelIllustration"/>. EF-managed table <c>dbo.ProductModelIllustrationAuditLogs</c>. A pure junction table — audit rows only carry the composite key plus timestamp.</summary>
public class ProductModelIllustrationAuditLog : AdventureWorksAuditLogBase
{
    public int ProductModelId { get; set; }
    public int IllustrationId { get; set; }

    public DateTime SourceModifiedDate { get; set; }
}
