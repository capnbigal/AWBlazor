using ElementaryApp.Models.AdventureWorks;
using FluentValidation;

namespace ElementaryApp.Validators.AdventureWorks;

public sealed class CreateCultureValidator : AbstractValidator<CreateCultureRequest>
{
    public CreateCultureValidator()
    {
        RuleFor(x => x.CultureId)
            .NotEmpty().WithMessage("Culture ID is required.")
            .MaximumLength(6).WithMessage("Culture ID must be at most 6 characters.");
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required.").MaximumLength(50);
    }
}

public sealed class UpdateCultureValidator : AbstractValidator<UpdateCultureRequest>
{
    public UpdateCultureValidator()
    {
        When(x => x.Name is not null, () =>
        {
            RuleFor(x => x.Name!).NotEmpty().WithMessage("Name cannot be blanked out.").MaximumLength(50);
        });
    }
}
