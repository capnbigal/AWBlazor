using AWBlazorApp.Features.Sales.CountryRegionCurrencies.Dtos; using AWBlazorApp.Features.Sales.CreditCards.Dtos; using AWBlazorApp.Features.Sales.Currencies.Dtos; using AWBlazorApp.Features.Sales.CurrencyRates.Dtos; using AWBlazorApp.Features.Sales.Customers.Dtos; using AWBlazorApp.Features.Sales.PersonCreditCards.Dtos; using AWBlazorApp.Features.Sales.SalesOrderDetails.Dtos; using AWBlazorApp.Features.Sales.SalesOrderHeaders.Dtos; using AWBlazorApp.Features.Sales.SalesOrderHeaderSalesReasons.Dtos; using AWBlazorApp.Features.Sales.SalesPeople.Dtos; using AWBlazorApp.Features.Sales.SalesPersonQuotaHistories.Dtos; using AWBlazorApp.Features.Sales.SalesReasons.Dtos; using AWBlazorApp.Features.Sales.SalesTaxRates.Dtos; using AWBlazorApp.Features.Sales.SalesTerritories.Dtos; using AWBlazorApp.Features.Sales.SalesTerritoryHistories.Dtos; using AWBlazorApp.Features.Sales.ShoppingCartItems.Dtos; using AWBlazorApp.Features.Sales.SpecialOffers.Dtos; using AWBlazorApp.Features.Sales.SpecialOfferProducts.Dtos; using AWBlazorApp.Features.Sales.Stores.Dtos; 
using FluentValidation;

namespace AWBlazorApp.Features.Sales.SalesTerritoryHistories.Application.Validators;

public sealed class CreateSalesTerritoryHistoryValidator : AbstractValidator<CreateSalesTerritoryHistoryRequest>
{
    public CreateSalesTerritoryHistoryValidator()
    {
        RuleFor(x => x.BusinessEntityId).GreaterThan(0).WithMessage("BusinessEntityId is required.");
        RuleFor(x => x.TerritoryId).GreaterThan(0).WithMessage("TerritoryId is required.");
        RuleFor(x => x.StartDate).NotEmpty();
        When(x => x.EndDate.HasValue, () =>
            RuleFor(x => x.EndDate!.Value).GreaterThan(x => x.StartDate)
                .WithMessage("EndDate must be after StartDate."));
    }
}

public sealed class UpdateSalesTerritoryHistoryValidator : AbstractValidator<UpdateSalesTerritoryHistoryRequest>
{
    public UpdateSalesTerritoryHistoryValidator()
    {
        // EndDate is the only updatable field — no further validation needed.
    }
}
