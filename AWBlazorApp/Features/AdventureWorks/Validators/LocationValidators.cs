using AWBlazorApp.Features.AdventureWorks.Models;
using FluentValidation;

namespace AWBlazorApp.Features.AdventureWorks.Validators;

public sealed class CreateLocationValidator : AbstractValidator<CreateLocationRequest>
{
    public CreateLocationValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required.").MaximumLength(50);
        RuleFor(x => x.CostRate).GreaterThanOrEqualTo(0).WithMessage("Cost rate cannot be negative.");
        RuleFor(x => x.Availability).GreaterThanOrEqualTo(0).WithMessage("Availability cannot be negative.");
    }
}

public sealed class UpdateLocationValidator : AbstractValidator<UpdateLocationRequest>
{
    public UpdateLocationValidator()
    {
        When(x => x.Name is not null, () =>
        {
            RuleFor(x => x.Name!).NotEmpty().WithMessage("Name cannot be blanked out.").MaximumLength(50);
        });
        When(x => x.CostRate.HasValue, () =>
        {
            RuleFor(x => x.CostRate!.Value).GreaterThanOrEqualTo(0).WithMessage("Cost rate cannot be negative.");
        });
        When(x => x.Availability.HasValue, () =>
        {
            RuleFor(x => x.Availability!.Value).GreaterThanOrEqualTo(0).WithMessage("Availability cannot be negative.");
        });
    }
}
