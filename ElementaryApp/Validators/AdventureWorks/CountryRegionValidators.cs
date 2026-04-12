using ElementaryApp.Models.AdventureWorks;
using FluentValidation;

namespace ElementaryApp.Validators.AdventureWorks;

public sealed class CreateCountryRegionValidator : AbstractValidator<CreateCountryRegionRequest>
{
    public CreateCountryRegionValidator()
    {
        RuleFor(x => x.CountryRegionCode)
            .NotEmpty().WithMessage("Country/region code is required.")
            .MaximumLength(3).WithMessage("Country/region code must be at most 3 characters.");
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required.").MaximumLength(50);
    }
}

public sealed class UpdateCountryRegionValidator : AbstractValidator<UpdateCountryRegionRequest>
{
    public UpdateCountryRegionValidator()
    {
        When(x => x.Name is not null, () =>
        {
            RuleFor(x => x.Name!).NotEmpty().WithMessage("Name cannot be blanked out.").MaximumLength(50);
        });
    }
}
