using AWBlazorApp.Shared.Domain;
using System.ComponentModel.DataAnnotations;

namespace AWBlazorApp.Features.Sales.Domain;

/// <summary>Audit log for <see cref="ShoppingCartItem"/>. EF-managed table <c>dbo.ShoppingCartItemAuditLogs</c>.</summary>
public class ShoppingCartItemAuditLog : AdventureWorksAuditLogBase
{
    public int ShoppingCartItemId { get; set; }

    [MaxLength(50)] public string? ShoppingCartId { get; set; }
    public int Quantity { get; set; }
    public int ProductId { get; set; }
    public DateTime DateCreated { get; set; }
    public DateTime SourceModifiedDate { get; set; }
}
