using AWBlazorApp.Features.AdventureWorks.Models;
using FluentValidation;

namespace AWBlazorApp.Features.AdventureWorks.Validators;

public sealed class CreateAddressTypeValidator : AbstractValidator<CreateAddressTypeRequest>
{
    public CreateAddressTypeValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(50);
    }
}

public sealed class UpdateAddressTypeValidator : AbstractValidator<UpdateAddressTypeRequest>
{
    public UpdateAddressTypeValidator()
    {
        // Name is optional on update (null = don't touch), but if present must be non-blank + short.
        When(x => x.Name is not null, () =>
        {
            RuleFor(x => x.Name!)
                .NotEmpty().WithMessage("Name cannot be blanked out.")
                .MaximumLength(50);
        });
    }
}
