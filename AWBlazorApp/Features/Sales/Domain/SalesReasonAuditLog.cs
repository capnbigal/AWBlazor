using AWBlazorApp.Features.AdventureWorks.Domain;
using System.ComponentModel.DataAnnotations;

namespace AWBlazorApp.Features.Sales.Domain;

/// <summary>Audit log for <see cref="SalesReason"/>. EF-managed table <c>dbo.SalesReasonAuditLogs</c>.</summary>
public class SalesReasonAuditLog : AdventureWorksAuditLogBase
{
    public int SalesReasonId { get; set; }

    [MaxLength(50)] public string? Name { get; set; }
    [MaxLength(50)] public string? ReasonType { get; set; }
    public DateTime SourceModifiedDate { get; set; }
}
