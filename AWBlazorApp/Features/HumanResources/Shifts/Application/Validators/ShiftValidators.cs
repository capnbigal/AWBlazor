using AWBlazorApp.Features.HumanResources.Departments.Dtos; using AWBlazorApp.Features.HumanResources.Employees.Dtos; using AWBlazorApp.Features.HumanResources.EmployeeDepartmentHistories.Dtos; using AWBlazorApp.Features.HumanResources.EmployeePayHistories.Dtos; using AWBlazorApp.Features.HumanResources.JobCandidates.Dtos; using AWBlazorApp.Features.HumanResources.Shifts.Dtos; 
using FluentValidation;

namespace AWBlazorApp.Features.HumanResources.Shifts.Application.Validators;

public sealed class CreateShiftValidator : AbstractValidator<CreateShiftRequest>
{
    public CreateShiftValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required.").MaximumLength(50);
        RuleFor(x => x.StartTime).NotNull().WithMessage("Start time is required.");
        RuleFor(x => x.EndTime).NotNull().WithMessage("End time is required.");
    }
}

public sealed class UpdateShiftValidator : AbstractValidator<UpdateShiftRequest>
{
    public UpdateShiftValidator()
    {
        When(x => x.Name is not null, () =>
        {
            RuleFor(x => x.Name!).NotEmpty().WithMessage("Name cannot be blanked out.").MaximumLength(50);
        });
    }
}
