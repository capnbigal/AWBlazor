using AWBlazorApp.Features.Sales.CountryRegionCurrencies.Dtos; using AWBlazorApp.Features.Sales.CreditCards.Dtos; using AWBlazorApp.Features.Sales.Currencies.Dtos; using AWBlazorApp.Features.Sales.CurrencyRates.Dtos; using AWBlazorApp.Features.Sales.Customers.Dtos; using AWBlazorApp.Features.Sales.PersonCreditCards.Dtos; using AWBlazorApp.Features.Sales.SalesOrderDetails.Dtos; using AWBlazorApp.Features.Sales.SalesOrderHeaders.Dtos; using AWBlazorApp.Features.Sales.SalesOrderHeaderSalesReasons.Dtos; using AWBlazorApp.Features.Sales.SalesPeople.Dtos; using AWBlazorApp.Features.Sales.SalesPersonQuotaHistories.Dtos; using AWBlazorApp.Features.Sales.SalesReasons.Dtos; using AWBlazorApp.Features.Sales.SalesTaxRates.Dtos; using AWBlazorApp.Features.Sales.SalesTerritories.Dtos; using AWBlazorApp.Features.Sales.SalesTerritoryHistories.Dtos; using AWBlazorApp.Features.Sales.ShoppingCartItems.Dtos; using AWBlazorApp.Features.Sales.SpecialOffers.Dtos; using AWBlazorApp.Features.Sales.SpecialOfferProducts.Dtos; using AWBlazorApp.Features.Sales.Stores.Dtos; 
using FluentValidation;

namespace AWBlazorApp.Features.Sales.Stores.Application.Validators;

public sealed class CreateStoreValidator : AbstractValidator<CreateStoreRequest>
{
    public CreateStoreValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("BusinessEntityId is required.");
        RuleFor(x => x.Name).NotEmpty().MaximumLength(50);
        When(x => x.SalesPersonId.HasValue, () =>
            RuleFor(x => x.SalesPersonId!.Value).GreaterThan(0));
    }
}

public sealed class UpdateStoreValidator : AbstractValidator<UpdateStoreRequest>
{
    public UpdateStoreValidator()
    {
        When(x => x.Name is not null, () => RuleFor(x => x.Name!).NotEmpty().MaximumLength(50));
        When(x => x.SalesPersonId.HasValue, () => RuleFor(x => x.SalesPersonId!.Value).GreaterThan(0));
    }
}
