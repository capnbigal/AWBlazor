using System.ComponentModel.DataAnnotations;

namespace AWBlazorApp.Data.Entities.AdventureWorks;

/// <summary>Audit log for <see cref="Employee"/>. EF-managed table <c>dbo.EmployeeAuditLogs</c>.</summary>
public class EmployeeAuditLog : AdventureWorksAuditLogBase
{
    public int EmployeeId { get; set; }

    [MaxLength(15)] public string? NationalIDNumber { get; set; }
    [MaxLength(256)] public string? LoginID { get; set; }
    [MaxLength(50)] public string? JobTitle { get; set; }
    public DateTime BirthDate { get; set; }
    [MaxLength(1)] public string? MaritalStatus { get; set; }
    [MaxLength(1)] public string? Gender { get; set; }
    public DateTime HireDate { get; set; }
    public bool SalariedFlag { get; set; }
    public bool CurrentFlag { get; set; }
    public short VacationHours { get; set; }
    public short SickLeaveHours { get; set; }
    public Guid RowGuid { get; set; }
    public DateTime SourceModifiedDate { get; set; }
}
