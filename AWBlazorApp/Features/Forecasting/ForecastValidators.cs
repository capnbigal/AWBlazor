using AWBlazorApp.Shared.Models;
using AWBlazorApp.Features.Forecasting.Models;
using FluentValidation;

namespace AWBlazorApp.Shared.Validators;

public sealed class CreateForecastValidator : AbstractValidator<CreateForecastRequest>
{
    public CreateForecastValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(1000);
        RuleFor(x => x.LookbackMonths).InclusiveBetween(3, 60)
            .WithMessage("Lookback must be between 3 and 60 months.");
        RuleFor(x => x.HorizonPeriods).InclusiveBetween(1, 24)
            .WithMessage("Horizon must be between 1 and 24 periods.");
        RuleFor(x => x.DataSource).IsInEnum();
        RuleFor(x => x.Method).IsInEnum();
        RuleFor(x => x.Granularity).IsInEnum();
    }
}

public sealed class UpdateForecastValidator : AbstractValidator<UpdateForecastRequest>
{
    public UpdateForecastValidator()
    {
        RuleFor(x => x.Name).MaximumLength(200).When(x => x.Name is not null);
        RuleFor(x => x.Description).MaximumLength(1000).When(x => x.Description is not null);
        RuleFor(x => x.LookbackMonths).InclusiveBetween(3, 60).When(x => x.LookbackMonths.HasValue);
        RuleFor(x => x.HorizonPeriods).InclusiveBetween(1, 24).When(x => x.HorizonPeriods.HasValue);
        RuleFor(x => x.Method).IsInEnum().When(x => x.Method.HasValue);
        RuleFor(x => x.Status).IsInEnum().When(x => x.Status.HasValue);
    }
}
