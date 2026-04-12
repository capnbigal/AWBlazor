using ElementaryApp.Models.AdventureWorks;
using FluentValidation;

namespace ElementaryApp.Validators.AdventureWorks;

public sealed class CreateUnitMeasureValidator : AbstractValidator<CreateUnitMeasureRequest>
{
    public CreateUnitMeasureValidator()
    {
        RuleFor(x => x.UnitMeasureCode)
            .NotEmpty().WithMessage("Unit measure code is required.")
            .MaximumLength(3).WithMessage("Unit measure code must be at most 3 characters.");
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required.").MaximumLength(50);
    }
}

public sealed class UpdateUnitMeasureValidator : AbstractValidator<UpdateUnitMeasureRequest>
{
    public UpdateUnitMeasureValidator()
    {
        When(x => x.Name is not null, () =>
        {
            RuleFor(x => x.Name!).NotEmpty().WithMessage("Name cannot be blanked out.").MaximumLength(50);
        });
    }
}
