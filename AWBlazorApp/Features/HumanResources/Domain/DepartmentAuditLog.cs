using AWBlazorApp.Shared.Domain;
using System.ComponentModel.DataAnnotations;

namespace AWBlazorApp.Features.HumanResources.Domain;

/// <summary>Audit log for <see cref="Department"/>. EF-managed table <c>dbo.DepartmentAuditLogs</c>.</summary>
public class DepartmentAuditLog : AdventureWorksAuditLogBase
{
    public short DepartmentId { get; set; }

    [MaxLength(50)] public string? Name { get; set; }
    [MaxLength(50)] public string? GroupName { get; set; }
    public DateTime SourceModifiedDate { get; set; }
}
