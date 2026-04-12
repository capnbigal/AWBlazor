using System.ComponentModel.DataAnnotations;

namespace ElementaryApp.Data.Entities.AdventureWorks;

/// <summary>Audit log for <see cref="AddressType"/>. EF-managed table <c>dbo.AddressTypeAuditLogs</c>.</summary>
public class AddressTypeAuditLog : AdventureWorksAuditLogBase
{
    public int AddressTypeId { get; set; }

    [MaxLength(50)] public string? Name { get; set; }
    public Guid RowGuid { get; set; }
    public DateTime SourceModifiedDate { get; set; }
}
