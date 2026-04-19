using AWBlazorApp.Features.Sales.CountryRegionCurrencies.Dtos; using AWBlazorApp.Features.Sales.CreditCards.Dtos; using AWBlazorApp.Features.Sales.Currencies.Dtos; using AWBlazorApp.Features.Sales.CurrencyRates.Dtos; using AWBlazorApp.Features.Sales.Customers.Dtos; using AWBlazorApp.Features.Sales.PersonCreditCards.Dtos; using AWBlazorApp.Features.Sales.SalesOrderDetails.Dtos; using AWBlazorApp.Features.Sales.SalesOrderHeaders.Dtos; using AWBlazorApp.Features.Sales.SalesOrderHeaderSalesReasons.Dtos; using AWBlazorApp.Features.Sales.SalesPeople.Dtos; using AWBlazorApp.Features.Sales.SalesPersonQuotaHistories.Dtos; using AWBlazorApp.Features.Sales.SalesReasons.Dtos; using AWBlazorApp.Features.Sales.SalesTaxRates.Dtos; using AWBlazorApp.Features.Sales.SalesTerritories.Dtos; using AWBlazorApp.Features.Sales.SalesTerritoryHistories.Dtos; using AWBlazorApp.Features.Sales.ShoppingCartItems.Dtos; using AWBlazorApp.Features.Sales.SpecialOffers.Dtos; using AWBlazorApp.Features.Sales.SpecialOfferProducts.Dtos; using AWBlazorApp.Features.Sales.Stores.Dtos; 
using FluentValidation;

namespace AWBlazorApp.Features.Sales.SpecialOfferProducts.Application.Validators;

public sealed class CreateSpecialOfferProductValidator : AbstractValidator<CreateSpecialOfferProductRequest>
{
    public CreateSpecialOfferProductValidator()
    {
        RuleFor(x => x.SpecialOfferId).GreaterThan(0).WithMessage("SpecialOfferId is required.");
        RuleFor(x => x.ProductId).GreaterThan(0).WithMessage("ProductId is required.");
    }
}

public sealed class UpdateSpecialOfferProductValidator : AbstractValidator<UpdateSpecialOfferProductRequest>
{
    public UpdateSpecialOfferProductValidator()
    {
        // No fields to validate — junction table.
    }
}
