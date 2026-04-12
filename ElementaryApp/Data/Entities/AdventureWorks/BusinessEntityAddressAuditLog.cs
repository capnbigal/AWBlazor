namespace ElementaryApp.Data.Entities.AdventureWorks;

/// <summary>Audit log for <see cref="BusinessEntityAddress"/>. EF-managed table <c>dbo.BusinessEntityAddressAuditLogs</c>. 3-column composite key.</summary>
public class BusinessEntityAddressAuditLog : AdventureWorksAuditLogBase
{
    public int BusinessEntityId { get; set; }
    public int AddressId { get; set; }
    public int AddressTypeId { get; set; }

    public Guid RowGuid { get; set; }
    public DateTime SourceModifiedDate { get; set; }
}
