using AWBlazorApp.Shared.Domain;
using System.ComponentModel.DataAnnotations;

namespace AWBlazorApp.Features.Person.Domain;

/// <summary>Audit log for <see cref="Address"/>. EF-managed table <c>dbo.AddressAuditLogs</c>.</summary>
public class AddressAuditLog : AdventureWorksAuditLogBase
{
    public int AddressId { get; set; }

    [MaxLength(60)] public string? AddressLine1 { get; set; }
    [MaxLength(60)] public string? AddressLine2 { get; set; }
    [MaxLength(30)] public string? City { get; set; }
    public int StateProvinceId { get; set; }
    [MaxLength(15)] public string? PostalCode { get; set; }
    public Guid RowGuid { get; set; }
    public DateTime SourceModifiedDate { get; set; }
}
