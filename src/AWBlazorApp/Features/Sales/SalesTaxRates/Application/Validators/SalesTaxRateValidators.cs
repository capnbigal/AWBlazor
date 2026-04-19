using AWBlazorApp.Features.Sales.CountryRegionCurrencies.Dtos; using AWBlazorApp.Features.Sales.CreditCards.Dtos; using AWBlazorApp.Features.Sales.Currencies.Dtos; using AWBlazorApp.Features.Sales.CurrencyRates.Dtos; using AWBlazorApp.Features.Sales.Customers.Dtos; using AWBlazorApp.Features.Sales.PersonCreditCards.Dtos; using AWBlazorApp.Features.Sales.SalesOrderDetails.Dtos; using AWBlazorApp.Features.Sales.SalesOrderHeaders.Dtos; using AWBlazorApp.Features.Sales.SalesOrderHeaderSalesReasons.Dtos; using AWBlazorApp.Features.Sales.SalesPeople.Dtos; using AWBlazorApp.Features.Sales.SalesPersonQuotaHistories.Dtos; using AWBlazorApp.Features.Sales.SalesReasons.Dtos; using AWBlazorApp.Features.Sales.SalesTaxRates.Dtos; using AWBlazorApp.Features.Sales.SalesTerritories.Dtos; using AWBlazorApp.Features.Sales.SalesTerritoryHistories.Dtos; using AWBlazorApp.Features.Sales.ShoppingCartItems.Dtos; using AWBlazorApp.Features.Sales.SpecialOffers.Dtos; using AWBlazorApp.Features.Sales.SpecialOfferProducts.Dtos; using AWBlazorApp.Features.Sales.Stores.Dtos; 
using FluentValidation;

namespace AWBlazorApp.Features.Sales.SalesTaxRates.Application.Validators;

public sealed class CreateSalesTaxRateValidator : AbstractValidator<CreateSalesTaxRateRequest>
{
    public CreateSalesTaxRateValidator()
    {
        RuleFor(x => x.StateProvinceId).GreaterThan(0);
        RuleFor(x => x.TaxType).InclusiveBetween((byte)1, (byte)3).WithMessage("TaxType must be 1 (state), 2 (federal), or 3 (shared).");
        RuleFor(x => x.TaxRate).GreaterThanOrEqualTo(0).WithMessage("Tax rate cannot be negative.");
        RuleFor(x => x.Name).NotEmpty().MaximumLength(50);
    }
}

public sealed class UpdateSalesTaxRateValidator : AbstractValidator<UpdateSalesTaxRateRequest>
{
    public UpdateSalesTaxRateValidator()
    {
        When(x => x.StateProvinceId.HasValue, () => RuleFor(x => x.StateProvinceId!.Value).GreaterThan(0));
        When(x => x.TaxType.HasValue, () => RuleFor(x => x.TaxType!.Value).InclusiveBetween((byte)1, (byte)3));
        When(x => x.TaxRate.HasValue, () => RuleFor(x => x.TaxRate!.Value).GreaterThanOrEqualTo(0));
        When(x => x.Name is not null, () => RuleFor(x => x.Name!).NotEmpty().MaximumLength(50));
    }
}
