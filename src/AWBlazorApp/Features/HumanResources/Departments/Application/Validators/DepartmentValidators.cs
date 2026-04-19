using AWBlazorApp.Features.HumanResources.Departments.Dtos; using AWBlazorApp.Features.HumanResources.Employees.Dtos; using AWBlazorApp.Features.HumanResources.EmployeeDepartmentHistories.Dtos; using AWBlazorApp.Features.HumanResources.EmployeePayHistories.Dtos; using AWBlazorApp.Features.HumanResources.JobCandidates.Dtos; using AWBlazorApp.Features.HumanResources.Shifts.Dtos; 
using FluentValidation;

namespace AWBlazorApp.Features.HumanResources.Departments.Application.Validators;

public sealed class CreateDepartmentValidator : AbstractValidator<CreateDepartmentRequest>
{
    public CreateDepartmentValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required.").MaximumLength(50);
        RuleFor(x => x.GroupName).NotEmpty().WithMessage("Group is required.").MaximumLength(50);
    }
}

public sealed class UpdateDepartmentValidator : AbstractValidator<UpdateDepartmentRequest>
{
    public UpdateDepartmentValidator()
    {
        When(x => x.Name is not null, () =>
        {
            RuleFor(x => x.Name!).NotEmpty().WithMessage("Name cannot be blanked out.").MaximumLength(50);
        });
        When(x => x.GroupName is not null, () =>
        {
            RuleFor(x => x.GroupName!).NotEmpty().WithMessage("Group cannot be blanked out.").MaximumLength(50);
        });
    }
}
