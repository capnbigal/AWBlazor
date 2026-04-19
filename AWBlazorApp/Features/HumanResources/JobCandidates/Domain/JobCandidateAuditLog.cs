using AWBlazorApp.Shared.Domain;
namespace AWBlazorApp.Features.HumanResources.JobCandidates.Domain;

/// <summary>Audit log for <see cref="JobCandidate"/>. EF-managed table <c>dbo.JobCandidateAuditLogs</c>.</summary>
public class JobCandidateAuditLog : AdventureWorksAuditLogBase
{
    public int JobCandidateId { get; set; }

    public int? BusinessEntityId { get; set; }
    public DateTime SourceModifiedDate { get; set; }
}
