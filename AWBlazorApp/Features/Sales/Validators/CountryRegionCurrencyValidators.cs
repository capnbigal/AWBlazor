using AWBlazorApp.Features.Sales.Dtos;
using FluentValidation;

namespace AWBlazorApp.Features.Sales.Validators;

public sealed class CreateCountryRegionCurrencyValidator : AbstractValidator<CreateCountryRegionCurrencyRequest>
{
    public CreateCountryRegionCurrencyValidator()
    {
        RuleFor(x => x.CountryRegionCode).NotEmpty().MaximumLength(3);
        RuleFor(x => x.CurrencyCode).NotEmpty().Length(3);
    }
}

public sealed class UpdateCountryRegionCurrencyValidator : AbstractValidator<UpdateCountryRegionCurrencyRequest>
{
    public UpdateCountryRegionCurrencyValidator()
    {
        // No fields to validate — junction table.
    }
}
