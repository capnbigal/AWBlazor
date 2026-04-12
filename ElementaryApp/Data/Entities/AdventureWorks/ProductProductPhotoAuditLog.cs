namespace ElementaryApp.Data.Entities.AdventureWorks;

/// <summary>Audit log for <see cref="ProductProductPhoto"/>. EF-managed table <c>dbo.ProductProductPhotoAuditLogs</c>. Carries both composite-key components.</summary>
public class ProductProductPhotoAuditLog : AdventureWorksAuditLogBase
{
    public int ProductId { get; set; }
    public int ProductPhotoId { get; set; }

    public bool Primary { get; set; }
    public DateTime SourceModifiedDate { get; set; }
}
