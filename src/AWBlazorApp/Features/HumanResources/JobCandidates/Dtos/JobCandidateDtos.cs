using AWBlazorApp.Features.HumanResources.Departments.Domain; using AWBlazorApp.Features.HumanResources.Employees.Domain; using AWBlazorApp.Features.HumanResources.EmployeeDepartmentHistories.Domain; using AWBlazorApp.Features.HumanResources.EmployeePayHistories.Domain; using AWBlazorApp.Features.HumanResources.JobCandidates.Domain; using AWBlazorApp.Features.HumanResources.Shifts.Domain; 

namespace AWBlazorApp.Features.HumanResources.JobCandidates.Dtos;

public sealed record JobCandidateDto(int Id, int? BusinessEntityId, DateTime ModifiedDate);

public sealed record CreateJobCandidateRequest
{
    public int? BusinessEntityId { get; set; }
}

public sealed record UpdateJobCandidateRequest
{
    public int? BusinessEntityId { get; set; }
}

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

    }
