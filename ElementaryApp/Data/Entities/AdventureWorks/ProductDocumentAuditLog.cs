using System.ComponentModel.DataAnnotations;

namespace ElementaryApp.Data.Entities.AdventureWorks;

/// <summary>Audit log for <see cref="ProductDocument"/>. EF-managed table <c>dbo.ProductDocumentAuditLogs</c>. Stores DocumentNode as string.</summary>
public class ProductDocumentAuditLog : AdventureWorksAuditLogBase
{
    public int ProductId { get; set; }

    /// <summary>String representation of the hierarchyid, e.g. "/1/2/".</summary>
    [MaxLength(256)]
    public string DocumentNode { get; set; } = string.Empty;

    public DateTime SourceModifiedDate { get; set; }
}
