using AWBlazorApp.Features.Sales.CountryRegionCurrencies.Dtos; using AWBlazorApp.Features.Sales.CreditCards.Dtos; using AWBlazorApp.Features.Sales.Currencies.Dtos; using AWBlazorApp.Features.Sales.CurrencyRates.Dtos; using AWBlazorApp.Features.Sales.Customers.Dtos; using AWBlazorApp.Features.Sales.PersonCreditCards.Dtos; using AWBlazorApp.Features.Sales.SalesOrderDetails.Dtos; using AWBlazorApp.Features.Sales.SalesOrderHeaders.Dtos; using AWBlazorApp.Features.Sales.SalesOrderHeaderSalesReasons.Dtos; using AWBlazorApp.Features.Sales.SalesPeople.Dtos; using AWBlazorApp.Features.Sales.SalesPersonQuotaHistories.Dtos; using AWBlazorApp.Features.Sales.SalesReasons.Dtos; using AWBlazorApp.Features.Sales.SalesTaxRates.Dtos; using AWBlazorApp.Features.Sales.SalesTerritories.Dtos; using AWBlazorApp.Features.Sales.SalesTerritoryHistories.Dtos; using AWBlazorApp.Features.Sales.ShoppingCartItems.Dtos; using AWBlazorApp.Features.Sales.SpecialOffers.Dtos; using AWBlazorApp.Features.Sales.SpecialOfferProducts.Dtos; using AWBlazorApp.Features.Sales.Stores.Dtos; 
using FluentValidation;

namespace AWBlazorApp.Features.Sales.SalesPersonQuotaHistories.Application.Validators;

public sealed class CreateSalesPersonQuotaHistoryValidator : AbstractValidator<CreateSalesPersonQuotaHistoryRequest>
{
    public CreateSalesPersonQuotaHistoryValidator()
    {
        RuleFor(x => x.BusinessEntityId).GreaterThan(0).WithMessage("BusinessEntityId is required.");
        RuleFor(x => x.QuotaDate).NotEmpty().WithMessage("QuotaDate is required.");
        RuleFor(x => x.SalesQuota).GreaterThanOrEqualTo(0).WithMessage("SalesQuota cannot be negative.");
    }
}

public sealed class UpdateSalesPersonQuotaHistoryValidator : AbstractValidator<UpdateSalesPersonQuotaHistoryRequest>
{
    public UpdateSalesPersonQuotaHistoryValidator()
    {
        When(x => x.SalesQuota.HasValue, () =>
            RuleFor(x => x.SalesQuota!.Value).GreaterThanOrEqualTo(0));
    }
}
