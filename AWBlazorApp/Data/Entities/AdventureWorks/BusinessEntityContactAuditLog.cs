namespace AWBlazorApp.Data.Entities.AdventureWorks;

/// <summary>Audit log for <see cref="BusinessEntityContact"/>. EF-managed table <c>dbo.BusinessEntityContactAuditLogs</c>. 3-column composite key.</summary>
public class BusinessEntityContactAuditLog : AdventureWorksAuditLogBase
{
    public int BusinessEntityId { get; set; }
    public int PersonId { get; set; }
    public int ContactTypeId { get; set; }

    public Guid RowGuid { get; set; }
    public DateTime SourceModifiedDate { get; set; }
}
