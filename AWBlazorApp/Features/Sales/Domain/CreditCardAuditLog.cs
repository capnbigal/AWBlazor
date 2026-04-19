using AWBlazorApp.Shared.Domain;
using System.ComponentModel.DataAnnotations;

namespace AWBlazorApp.Features.Sales.Domain;

/// <summary>Audit log for <see cref="CreditCard"/>. EF-managed table <c>dbo.CreditCardAuditLogs</c>.</summary>
public class CreditCardAuditLog : AdventureWorksAuditLogBase
{
    public int CreditCardId { get; set; }

    [MaxLength(50)] public string? CardType { get; set; }
    [MaxLength(25)] public string? CardNumber { get; set; }
    public byte ExpMonth { get; set; }
    public short ExpYear { get; set; }
    public DateTime SourceModifiedDate { get; set; }
}
