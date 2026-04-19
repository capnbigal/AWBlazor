using AWBlazorApp.Features.Sales.CountryRegionCurrencies.Dtos; using AWBlazorApp.Features.Sales.CreditCards.Dtos; using AWBlazorApp.Features.Sales.Currencies.Dtos; using AWBlazorApp.Features.Sales.CurrencyRates.Dtos; using AWBlazorApp.Features.Sales.Customers.Dtos; using AWBlazorApp.Features.Sales.PersonCreditCards.Dtos; using AWBlazorApp.Features.Sales.SalesOrderDetails.Dtos; using AWBlazorApp.Features.Sales.SalesOrderHeaders.Dtos; using AWBlazorApp.Features.Sales.SalesOrderHeaderSalesReasons.Dtos; using AWBlazorApp.Features.Sales.SalesPeople.Dtos; using AWBlazorApp.Features.Sales.SalesPersonQuotaHistories.Dtos; using AWBlazorApp.Features.Sales.SalesReasons.Dtos; using AWBlazorApp.Features.Sales.SalesTaxRates.Dtos; using AWBlazorApp.Features.Sales.SalesTerritories.Dtos; using AWBlazorApp.Features.Sales.SalesTerritoryHistories.Dtos; using AWBlazorApp.Features.Sales.ShoppingCartItems.Dtos; using AWBlazorApp.Features.Sales.SpecialOffers.Dtos; using AWBlazorApp.Features.Sales.SpecialOfferProducts.Dtos; using AWBlazorApp.Features.Sales.Stores.Dtos; 
using FluentValidation;

namespace AWBlazorApp.Features.Sales.ShoppingCartItems.Application.Validators;

public sealed class CreateShoppingCartItemValidator : AbstractValidator<CreateShoppingCartItemRequest>
{
    public CreateShoppingCartItemValidator()
    {
        RuleFor(x => x.ShoppingCartId).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Quantity).GreaterThan(0).WithMessage("Quantity must be positive.");
        RuleFor(x => x.ProductId).GreaterThan(0).WithMessage("Product id is required.");
    }
}

public sealed class UpdateShoppingCartItemValidator : AbstractValidator<UpdateShoppingCartItemRequest>
{
    public UpdateShoppingCartItemValidator()
    {
        When(x => x.ShoppingCartId is not null, () => RuleFor(x => x.ShoppingCartId!).NotEmpty().MaximumLength(50));
        When(x => x.Quantity.HasValue, () => RuleFor(x => x.Quantity!.Value).GreaterThan(0));
        When(x => x.ProductId.HasValue, () => RuleFor(x => x.ProductId!.Value).GreaterThan(0));
    }
}
