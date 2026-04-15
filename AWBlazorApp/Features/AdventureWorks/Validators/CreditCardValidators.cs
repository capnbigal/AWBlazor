using AWBlazorApp.Features.AdventureWorks.Models;
using FluentValidation;

namespace AWBlazorApp.Features.AdventureWorks.Validators;

public sealed class CreateCreditCardValidator : AbstractValidator<CreateCreditCardRequest>
{
    public CreateCreditCardValidator()
    {
        RuleFor(x => x.CardType).NotEmpty().MaximumLength(50);
        RuleFor(x => x.CardNumber).NotEmpty().MaximumLength(25);
        RuleFor(x => x.ExpMonth).InclusiveBetween((byte)1, (byte)12).WithMessage("ExpMonth must be 1–12.");
        RuleFor(x => x.ExpYear).GreaterThanOrEqualTo((short)2000).WithMessage("ExpYear must be >= 2000.");
    }
}

public sealed class UpdateCreditCardValidator : AbstractValidator<UpdateCreditCardRequest>
{
    public UpdateCreditCardValidator()
    {
        When(x => x.CardType is not null, () => RuleFor(x => x.CardType!).NotEmpty().MaximumLength(50));
        When(x => x.CardNumber is not null, () => RuleFor(x => x.CardNumber!).NotEmpty().MaximumLength(25));
        When(x => x.ExpMonth.HasValue, () => RuleFor(x => x.ExpMonth!.Value).InclusiveBetween((byte)1, (byte)12));
        When(x => x.ExpYear.HasValue, () => RuleFor(x => x.ExpYear!.Value).GreaterThanOrEqualTo((short)2000));
    }
}
