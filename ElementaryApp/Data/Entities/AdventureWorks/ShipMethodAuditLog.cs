using System.ComponentModel.DataAnnotations;

namespace ElementaryApp.Data.Entities.AdventureWorks;

/// <summary>Audit log for <see cref="ShipMethod"/>. EF-managed table <c>dbo.ShipMethodAuditLogs</c>.</summary>
public class ShipMethodAuditLog : AdventureWorksAuditLogBase
{
    public int ShipMethodId { get; set; }

    [MaxLength(50)] public string? Name { get; set; }
    public decimal ShipBase { get; set; }
    public decimal ShipRate { get; set; }
    public Guid RowGuid { get; set; }
    public DateTime SourceModifiedDate { get; set; }
}
