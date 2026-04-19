using AWBlazorApp.Features.Sales.CountryRegionCurrencies.Dtos; using AWBlazorApp.Features.Sales.CreditCards.Dtos; using AWBlazorApp.Features.Sales.Currencies.Dtos; using AWBlazorApp.Features.Sales.CurrencyRates.Dtos; using AWBlazorApp.Features.Sales.Customers.Dtos; using AWBlazorApp.Features.Sales.PersonCreditCards.Dtos; using AWBlazorApp.Features.Sales.SalesOrderDetails.Dtos; using AWBlazorApp.Features.Sales.SalesOrderHeaders.Dtos; using AWBlazorApp.Features.Sales.SalesOrderHeaderSalesReasons.Dtos; using AWBlazorApp.Features.Sales.SalesPeople.Dtos; using AWBlazorApp.Features.Sales.SalesPersonQuotaHistories.Dtos; using AWBlazorApp.Features.Sales.SalesReasons.Dtos; using AWBlazorApp.Features.Sales.SalesTaxRates.Dtos; using AWBlazorApp.Features.Sales.SalesTerritories.Dtos; using AWBlazorApp.Features.Sales.SalesTerritoryHistories.Dtos; using AWBlazorApp.Features.Sales.ShoppingCartItems.Dtos; using AWBlazorApp.Features.Sales.SpecialOffers.Dtos; using AWBlazorApp.Features.Sales.SpecialOfferProducts.Dtos; using AWBlazorApp.Features.Sales.Stores.Dtos; 
using FluentValidation;

namespace AWBlazorApp.Features.Sales.Customers.Application.Validators;

public sealed class CreateCustomerValidator : AbstractValidator<CreateCustomerRequest>
{
    public CreateCustomerValidator()
    {
        // A customer is either a person (PersonID) or a store (StoreID), exclusive.
        RuleFor(x => x).Must(x => (x.PersonId.HasValue ^ x.StoreId.HasValue))
            .WithMessage("Customer must be either a person (PersonId) or a store (StoreId), not both and not neither.");
        When(x => x.PersonId.HasValue, () =>
            RuleFor(x => x.PersonId!.Value).GreaterThan(0));
        When(x => x.StoreId.HasValue, () =>
            RuleFor(x => x.StoreId!.Value).GreaterThan(0));
        When(x => x.TerritoryId.HasValue, () =>
            RuleFor(x => x.TerritoryId!.Value).GreaterThan(0));
    }
}

public sealed class UpdateCustomerValidator : AbstractValidator<UpdateCustomerRequest>
{
    public UpdateCustomerValidator()
    {
        When(x => x.PersonId.HasValue, () => RuleFor(x => x.PersonId!.Value).GreaterThan(0));
        When(x => x.StoreId.HasValue, () => RuleFor(x => x.StoreId!.Value).GreaterThan(0));
        When(x => x.TerritoryId.HasValue, () => RuleFor(x => x.TerritoryId!.Value).GreaterThan(0));
    }
}
