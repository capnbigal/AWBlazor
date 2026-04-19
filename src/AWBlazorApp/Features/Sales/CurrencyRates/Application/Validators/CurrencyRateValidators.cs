using AWBlazorApp.Features.Sales.CountryRegionCurrencies.Dtos; using AWBlazorApp.Features.Sales.CreditCards.Dtos; using AWBlazorApp.Features.Sales.Currencies.Dtos; using AWBlazorApp.Features.Sales.CurrencyRates.Dtos; using AWBlazorApp.Features.Sales.Customers.Dtos; using AWBlazorApp.Features.Sales.PersonCreditCards.Dtos; using AWBlazorApp.Features.Sales.SalesOrderDetails.Dtos; using AWBlazorApp.Features.Sales.SalesOrderHeaders.Dtos; using AWBlazorApp.Features.Sales.SalesOrderHeaderSalesReasons.Dtos; using AWBlazorApp.Features.Sales.SalesPeople.Dtos; using AWBlazorApp.Features.Sales.SalesPersonQuotaHistories.Dtos; using AWBlazorApp.Features.Sales.SalesReasons.Dtos; using AWBlazorApp.Features.Sales.SalesTaxRates.Dtos; using AWBlazorApp.Features.Sales.SalesTerritories.Dtos; using AWBlazorApp.Features.Sales.SalesTerritoryHistories.Dtos; using AWBlazorApp.Features.Sales.ShoppingCartItems.Dtos; using AWBlazorApp.Features.Sales.SpecialOffers.Dtos; using AWBlazorApp.Features.Sales.SpecialOfferProducts.Dtos; using AWBlazorApp.Features.Sales.Stores.Dtos; 
using FluentValidation;

namespace AWBlazorApp.Features.Sales.CurrencyRates.Application.Validators;

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
