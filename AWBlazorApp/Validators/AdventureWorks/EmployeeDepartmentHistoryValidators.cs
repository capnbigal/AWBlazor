using AWBlazorApp.Models.AdventureWorks;
using FluentValidation;

namespace AWBlazorApp.Validators.AdventureWorks;

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
