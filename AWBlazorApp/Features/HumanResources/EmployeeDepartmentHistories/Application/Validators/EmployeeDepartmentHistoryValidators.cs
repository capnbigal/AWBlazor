using AWBlazorApp.Features.HumanResources.Departments.Dtos; using AWBlazorApp.Features.HumanResources.Employees.Dtos; using AWBlazorApp.Features.HumanResources.EmployeeDepartmentHistories.Dtos; using AWBlazorApp.Features.HumanResources.EmployeePayHistories.Dtos; using AWBlazorApp.Features.HumanResources.JobCandidates.Dtos; using AWBlazorApp.Features.HumanResources.Shifts.Dtos; 
using FluentValidation;

namespace AWBlazorApp.Features.HumanResources.EmployeeDepartmentHistories.Application.Validators;

public sealed class CreateEmployeeDepartmentHistoryValidator : AbstractValidator<CreateEmployeeDepartmentHistoryRequest>
{
    public CreateEmployeeDepartmentHistoryValidator()
    {
        RuleFor(x => x.BusinessEntityId).GreaterThan(0).WithMessage("BusinessEntityId is required.");
        RuleFor(x => x.DepartmentId).GreaterThan((short)0).WithMessage("DepartmentId is required.");
        RuleFor(x => x.ShiftId).GreaterThan((byte)0).WithMessage("ShiftId is required.");
        RuleFor(x => x.StartDate).NotEmpty();
        When(x => x.EndDate.HasValue, () =>
            RuleFor(x => x.EndDate!.Value).GreaterThanOrEqualTo(x => x.StartDate)
                .WithMessage("EndDate cannot be before StartDate."));
    }
}

public sealed class UpdateEmployeeDepartmentHistoryValidator : AbstractValidator<UpdateEmployeeDepartmentHistoryRequest>
{
    public UpdateEmployeeDepartmentHistoryValidator()
    {
        // EndDate is the only mutable field — no further constraints possible without re-fetching.
    }
}
