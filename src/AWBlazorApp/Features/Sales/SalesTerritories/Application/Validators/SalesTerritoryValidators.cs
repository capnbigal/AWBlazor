using AWBlazorApp.Features.Sales.CountryRegionCurrencies.Dtos; using AWBlazorApp.Features.Sales.CreditCards.Dtos; using AWBlazorApp.Features.Sales.Currencies.Dtos; using AWBlazorApp.Features.Sales.CurrencyRates.Dtos; using AWBlazorApp.Features.Sales.Customers.Dtos; using AWBlazorApp.Features.Sales.PersonCreditCards.Dtos; using AWBlazorApp.Features.Sales.SalesOrderDetails.Dtos; using AWBlazorApp.Features.Sales.SalesOrderHeaders.Dtos; using AWBlazorApp.Features.Sales.SalesOrderHeaderSalesReasons.Dtos; using AWBlazorApp.Features.Sales.SalesPeople.Dtos; using AWBlazorApp.Features.Sales.SalesPersonQuotaHistories.Dtos; using AWBlazorApp.Features.Sales.SalesReasons.Dtos; using AWBlazorApp.Features.Sales.SalesTaxRates.Dtos; using AWBlazorApp.Features.Sales.SalesTerritories.Dtos; using AWBlazorApp.Features.Sales.SalesTerritoryHistories.Dtos; using AWBlazorApp.Features.Sales.ShoppingCartItems.Dtos; using AWBlazorApp.Features.Sales.SpecialOffers.Dtos; using AWBlazorApp.Features.Sales.SpecialOfferProducts.Dtos; using AWBlazorApp.Features.Sales.Stores.Dtos; 
using FluentValidation;

namespace AWBlazorApp.Features.Sales.SalesTerritories.Application.Validators;

public sealed class CreateSalesTerritoryValidator : AbstractValidator<CreateSalesTerritoryRequest>
{
    public CreateSalesTerritoryValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required.").MaximumLength(50);
        RuleFor(x => x.CountryRegionCode).NotEmpty().WithMessage("Country/region code is required.").MaximumLength(3);
        RuleFor(x => x.GroupName).NotEmpty().WithMessage("Group is required.").MaximumLength(50);
    }
}

public sealed class UpdateSalesTerritoryValidator : AbstractValidator<UpdateSalesTerritoryRequest>
{
    public UpdateSalesTerritoryValidator()
    {
        When(x => x.Name is not null, () => RuleFor(x => x.Name!).NotEmpty().MaximumLength(50));
        When(x => x.CountryRegionCode is not null, () => RuleFor(x => x.CountryRegionCode!).NotEmpty().MaximumLength(3));
        When(x => x.GroupName is not null, () => RuleFor(x => x.GroupName!).NotEmpty().MaximumLength(50));
    }
}
