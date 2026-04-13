using AWBlazorApp.Data.Entities.AdventureWorks;

namespace AWBlazorApp.Models.AdventureWorks;

public sealed record JobCandidateDto(int Id, int? BusinessEntityId, DateTime ModifiedDate);

public sealed record CreateJobCandidateRequest
{
    public int? BusinessEntityId { get; set; }
}

public sealed record UpdateJobCandidateRequest
{
    public int? BusinessEntityId { get; set; }
}

public sealed record JobCandidateAuditLogDto(
    int Id, int JobCandidateId, string Action, string? ChangedBy, DateTime ChangedDate,
    string? ChangeSummary, int? BusinessEntityId, DateTime SourceModifiedDate);

public static class JobCandidateMappings
{
    public static JobCandidateDto ToDto(this JobCandidate e) => new(e.Id, e.BusinessEntityId, e.ModifiedDate);

    public static JobCandidate ToEntity(this CreateJobCandidateRequest r) => new()
    {
        BusinessEntityId = r.BusinessEntityId,
        ModifiedDate = DateTime.UtcNow,
    };

    public static void ApplyTo(this UpdateJobCandidateRequest r, JobCandidate e)
    {
        if (r.BusinessEntityId.HasValue) e.BusinessEntityId = r.BusinessEntityId.Value;
        e.ModifiedDate = DateTime.UtcNow;
    }

    public static JobCandidateAuditLogDto ToDto(this JobCandidateAuditLog a) => new(
        a.Id, a.JobCandidateId, a.Action, a.ChangedBy, a.ChangedDate, a.ChangeSummary,
        a.BusinessEntityId, a.SourceModifiedDate);
}
