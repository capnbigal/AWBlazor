using AWBlazorApp.Features.Sales.CountryRegionCurrencies.Dtos; using AWBlazorApp.Features.Sales.CreditCards.Dtos; using AWBlazorApp.Features.Sales.Currencies.Dtos; using AWBlazorApp.Features.Sales.CurrencyRates.Dtos; using AWBlazorApp.Features.Sales.Customers.Dtos; using AWBlazorApp.Features.Sales.PersonCreditCards.Dtos; using AWBlazorApp.Features.Sales.SalesOrderDetails.Dtos; using AWBlazorApp.Features.Sales.SalesOrderHeaders.Dtos; using AWBlazorApp.Features.Sales.SalesOrderHeaderSalesReasons.Dtos; using AWBlazorApp.Features.Sales.SalesPeople.Dtos; using AWBlazorApp.Features.Sales.SalesPersonQuotaHistories.Dtos; using AWBlazorApp.Features.Sales.SalesReasons.Dtos; using AWBlazorApp.Features.Sales.SalesTaxRates.Dtos; using AWBlazorApp.Features.Sales.SalesTerritories.Dtos; using AWBlazorApp.Features.Sales.SalesTerritoryHistories.Dtos; using AWBlazorApp.Features.Sales.ShoppingCartItems.Dtos; using AWBlazorApp.Features.Sales.SpecialOffers.Dtos; using AWBlazorApp.Features.Sales.SpecialOfferProducts.Dtos; using AWBlazorApp.Features.Sales.Stores.Dtos; 
using FluentValidation;

namespace AWBlazorApp.Features.Sales.SalesPeople.Application.Validators;

public sealed class CreateSalesPersonValidator : AbstractValidator<CreateSalesPersonRequest>
{
    public CreateSalesPersonValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("BusinessEntityId is required (must already exist in Person.BusinessEntity).");
        RuleFor(x => x.Bonus).GreaterThanOrEqualTo(0).WithMessage("Bonus cannot be negative.");
        RuleFor(x => x.CommissionPct).InclusiveBetween(0m, 1m).WithMessage("Commission must be between 0.0 and 1.0.");
        When(x => x.SalesQuota.HasValue, () =>
            RuleFor(x => x.SalesQuota!.Value).GreaterThanOrEqualTo(0));
        When(x => x.TerritoryId.HasValue, () =>
            RuleFor(x => x.TerritoryId!.Value).GreaterThan(0));
    }
}

public sealed class UpdateSalesPersonValidator : AbstractValidator<UpdateSalesPersonRequest>
{
    public UpdateSalesPersonValidator()
    {
        When(x => x.Bonus.HasValue, () => RuleFor(x => x.Bonus!.Value).GreaterThanOrEqualTo(0));
        When(x => x.CommissionPct.HasValue, () => RuleFor(x => x.CommissionPct!.Value).InclusiveBetween(0m, 1m));
        When(x => x.SalesQuota.HasValue, () => RuleFor(x => x.SalesQuota!.Value).GreaterThanOrEqualTo(0));
        When(x => x.TerritoryId.HasValue, () => RuleFor(x => x.TerritoryId!.Value).GreaterThan(0));
    }
}
