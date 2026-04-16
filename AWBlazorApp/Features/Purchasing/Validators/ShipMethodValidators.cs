using AWBlazorApp.Features.Purchasing.Models;
using FluentValidation;

namespace AWBlazorApp.Features.Purchasing.Validators;

public sealed class CreateShipMethodValidator : AbstractValidator<CreateShipMethodRequest>
{
    public CreateShipMethodValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required.").MaximumLength(50);
        RuleFor(x => x.ShipBase).GreaterThanOrEqualTo(0).WithMessage("Ship base cannot be negative.");
        RuleFor(x => x.ShipRate).GreaterThanOrEqualTo(0).WithMessage("Ship rate cannot be negative.");
    }
}

public sealed class UpdateShipMethodValidator : AbstractValidator<UpdateShipMethodRequest>
{
    public UpdateShipMethodValidator()
    {
        When(x => x.Name is not null, () =>
        {
            RuleFor(x => x.Name!).NotEmpty().WithMessage("Name cannot be blanked out.").MaximumLength(50);
        });
        When(x => x.ShipBase.HasValue, () =>
        {
            RuleFor(x => x.ShipBase!.Value).GreaterThanOrEqualTo(0).WithMessage("Ship base cannot be negative.");
        });
        When(x => x.ShipRate.HasValue, () =>
        {
            RuleFor(x => x.ShipRate!.Value).GreaterThanOrEqualTo(0).WithMessage("Ship rate cannot be negative.");
        });
    }
}
