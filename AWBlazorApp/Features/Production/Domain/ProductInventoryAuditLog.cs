using AWBlazorApp.Shared.Domain;
using System.ComponentModel.DataAnnotations;

namespace AWBlazorApp.Features.Production.Domain;

/// <summary>Audit log for <see cref="ProductInventory"/>. EF-managed table <c>dbo.ProductInventoryAuditLogs</c>. Carries both composite-key components.</summary>
public class ProductInventoryAuditLog : AdventureWorksAuditLogBase
{
    public int ProductId { get; set; }
    public short LocationId { get; set; }

    [MaxLength(10)] public string? Shelf { get; set; }
    public byte Bin { get; set; }
    public short Quantity { get; set; }
    public Guid RowGuid { get; set; }
    public DateTime SourceModifiedDate { get; set; }
}
