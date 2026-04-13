using AWBlazorApp.Models.AdventureWorks;
using FluentValidation;

namespace AWBlazorApp.Validators.AdventureWorks;

public sealed class CreateCurrencyRateValidator : AbstractValidator<CreateCurrencyRateRequest>
{
    public CreateCurrencyRateValidator()
    {
        RuleFor(x => x.FromCurrencyCode).NotEmpty().Length(3);
        RuleFor(x => x.ToCurrencyCode).NotEmpty().Length(3);
        RuleFor(x => x.AverageRate).GreaterThan(0).WithMessage("AverageRate must be positive.");
        RuleFor(x => x.EndOfDayRate).GreaterThan(0).WithMessage("EndOfDayRate must be positive.");
        RuleFor(x => x).Must(x =>
                !string.IsNullOrEmpty(x.FromCurrencyCode) && !string.IsNullOrEmpty(x.ToCurrencyCode) &&
                !string.Equals(x.FromCurrencyCode, x.ToCurrencyCode, StringComparison.OrdinalIgnoreCase))
            .WithMessage("FromCurrencyCode and ToCurrencyCode must differ.");
    }
}

public sealed class UpdateCurrencyRateValidator : AbstractValidator<UpdateCurrencyRateRequest>
{
    public UpdateCurrencyRateValidator()
    {
        When(x => x.FromCurrencyCode is not null, () => RuleFor(x => x.FromCurrencyCode!).NotEmpty().Length(3));
        When(x => x.ToCurrencyCode is not null, () => RuleFor(x => x.ToCurrencyCode!).NotEmpty().Length(3));
        When(x => x.AverageRate.HasValue, () => RuleFor(x => x.AverageRate!.Value).GreaterThan(0));
        When(x => x.EndOfDayRate.HasValue, () => RuleFor(x => x.EndOfDayRate!.Value).GreaterThan(0));
    }
}
