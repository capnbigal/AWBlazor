using AWBlazorApp.Features.HumanResources.Departments.Dtos; using AWBlazorApp.Features.HumanResources.Employees.Dtos; using AWBlazorApp.Features.HumanResources.EmployeeDepartmentHistories.Dtos; using AWBlazorApp.Features.HumanResources.EmployeePayHistories.Dtos; using AWBlazorApp.Features.HumanResources.JobCandidates.Dtos; using AWBlazorApp.Features.HumanResources.Shifts.Dtos; 
using FluentValidation;

namespace AWBlazorApp.Features.HumanResources.JobCandidates.Application.Validators;

public sealed class CreateJobCandidateValidator : AbstractValidator<CreateJobCandidateRequest>
{
    public CreateJobCandidateValidator()
    {
        When(x => x.BusinessEntityId.HasValue, () =>
            RuleFor(x => x.BusinessEntityId!.Value).GreaterThan(0));
    }
}

public sealed class UpdateJobCandidateValidator : AbstractValidator<UpdateJobCandidateRequest>
{
    public UpdateJobCandidateValidator()
    {
        When(x => x.BusinessEntityId.HasValue, () =>
            RuleFor(x => x.BusinessEntityId!.Value).GreaterThan(0));
    }
}
