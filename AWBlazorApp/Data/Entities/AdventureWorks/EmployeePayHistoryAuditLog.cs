namespace AWBlazorApp.Data.Entities.AdventureWorks;

/// <summary>Audit log for <see cref="EmployeePayHistory"/>. EF-managed table <c>dbo.EmployeePayHistoryAuditLogs</c>.</summary>
public class EmployeePayHistoryAuditLog : AdventureWorksAuditLogBase
{
    public int BusinessEntityId { get; set; }
    public DateTime RateChangeDate { get; set; }

    public decimal Rate { get; set; }
    public byte PayFrequency { get; set; }
    public DateTime SourceModifiedDate { get; set; }
}
