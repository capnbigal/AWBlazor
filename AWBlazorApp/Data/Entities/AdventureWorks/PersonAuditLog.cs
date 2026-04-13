using System.ComponentModel.DataAnnotations;

namespace AWBlazorApp.Data.Entities.AdventureWorks;

/// <summary>Audit log for <see cref="Person"/>. EF-managed table <c>dbo.PersonAuditLogs</c>.</summary>
public class PersonAuditLog : AdventureWorksAuditLogBase
{
    public int PersonId { get; set; }

    [MaxLength(2)] public string? PersonType { get; set; }
    public bool NameStyle { get; set; }
    [MaxLength(8)] public string? Title { get; set; }
    [MaxLength(50)] public string? FirstName { get; set; }
    [MaxLength(50)] public string? MiddleName { get; set; }
    [MaxLength(50)] public string? LastName { get; set; }
    [MaxLength(10)] public string? Suffix { get; set; }
    public int EmailPromotion { get; set; }
    public Guid RowGuid { get; set; }
    public DateTime SourceModifiedDate { get; set; }
}
