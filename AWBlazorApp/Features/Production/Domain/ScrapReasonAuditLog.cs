using AWBlazorApp.Shared.Domain;
using System.ComponentModel.DataAnnotations;

namespace AWBlazorApp.Features.Production.Domain;

/// <summary>Audit log for <see cref="ScrapReason"/>. EF-managed table <c>dbo.ScrapReasonAuditLogs</c>.</summary>
public class ScrapReasonAuditLog : AdventureWorksAuditLogBase
{
    public short ScrapReasonId { get; set; }

    [MaxLength(50)] public string? Name { get; set; }
    public DateTime SourceModifiedDate { get; set; }
}
