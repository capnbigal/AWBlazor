using AWBlazorApp.Shared.Domain;
namespace AWBlazorApp.Features.Person.Domain;

/// <summary>Audit log for <see cref="BusinessEntity"/>. EF-managed table <c>dbo.BusinessEntityAuditLogs</c>.</summary>
public class BusinessEntityAuditLog : AdventureWorksAuditLogBase
{
    public int BusinessEntityId { get; set; }

    public Guid RowGuid { get; set; }
    public DateTime SourceModifiedDate { get; set; }
}
