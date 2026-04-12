using System.ComponentModel.DataAnnotations;

namespace ElementaryApp.Data.Entities.AdventureWorks;

/// <summary>Audit log for <see cref="Shift"/>. EF-managed table <c>dbo.ShiftAuditLogs</c>.</summary>
public class ShiftAuditLog : AdventureWorksAuditLogBase
{
    public byte ShiftId { get; set; }

    [MaxLength(50)] public string? Name { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public DateTime SourceModifiedDate { get; set; }
}
