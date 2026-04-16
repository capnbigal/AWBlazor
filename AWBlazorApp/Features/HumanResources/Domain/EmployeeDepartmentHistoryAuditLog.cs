using AWBlazorApp.Features.AdventureWorks.Domain;
namespace AWBlazorApp.Features.HumanResources.Domain;

/// <summary>Audit log for <see cref="EmployeeDepartmentHistory"/>. EF-managed table <c>dbo.EmployeeDepartmentHistoryAuditLogs</c>. Carries all 4 composite-key components.</summary>
public class EmployeeDepartmentHistoryAuditLog : AdventureWorksAuditLogBase
{
    public int BusinessEntityId { get; set; }
    public short DepartmentId { get; set; }
    public byte ShiftId { get; set; }
    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }
    public DateTime SourceModifiedDate { get; set; }
}
