using AWBlazorApp.Models.AdventureWorks;
using FluentValidation;

namespace AWBlazorApp.Validators.AdventureWorks;

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
