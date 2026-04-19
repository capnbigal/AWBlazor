using AWBlazorApp.Features.HumanResources.Departments.Dtos; using AWBlazorApp.Features.HumanResources.Employees.Dtos; using AWBlazorApp.Features.HumanResources.EmployeeDepartmentHistories.Dtos; using AWBlazorApp.Features.HumanResources.EmployeePayHistories.Dtos; using AWBlazorApp.Features.HumanResources.JobCandidates.Dtos; using AWBlazorApp.Features.HumanResources.Shifts.Dtos; 
using FluentValidation;

namespace AWBlazorApp.Features.HumanResources.EmployeePayHistories.Application.Validators;

public sealed class CreateEmployeePayHistoryValidator : AbstractValidator<CreateEmployeePayHistoryRequest>
{
    public CreateEmployeePayHistoryValidator()
    {
        RuleFor(x => x.BusinessEntityId).GreaterThan(0).WithMessage("BusinessEntityId is required.");
        RuleFor(x => x.RateChangeDate).NotEmpty();
        RuleFor(x => x.Rate).GreaterThan(0).WithMessage("Rate must be greater than zero.");
        RuleFor(x => x.PayFrequency).InclusiveBetween((byte)1, (byte)2)
            .WithMessage("PayFrequency must be 1 (Monthly) or 2 (Biweekly).");
    }
}

public sealed class UpdateEmployeePayHistoryValidator : AbstractValidator<UpdateEmployeePayHistoryRequest>
{
    public UpdateEmployeePayHistoryValidator()
    {
        When(x => x.Rate.HasValue, () => RuleFor(x => x.Rate!.Value).GreaterThan(0));
        When(x => x.PayFrequency.HasValue, () =>
            RuleFor(x => x.PayFrequency!.Value).InclusiveBetween((byte)1, (byte)2));
    }
}
