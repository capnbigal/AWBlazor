using AWBlazorApp.Features.Sales.CountryRegionCurrencies.Dtos; using AWBlazorApp.Features.Sales.CreditCards.Dtos; using AWBlazorApp.Features.Sales.Currencies.Dtos; using AWBlazorApp.Features.Sales.CurrencyRates.Dtos; using AWBlazorApp.Features.Sales.Customers.Dtos; using AWBlazorApp.Features.Sales.PersonCreditCards.Dtos; using AWBlazorApp.Features.Sales.SalesOrderDetails.Dtos; using AWBlazorApp.Features.Sales.SalesOrderHeaders.Dtos; using AWBlazorApp.Features.Sales.SalesOrderHeaderSalesReasons.Dtos; using AWBlazorApp.Features.Sales.SalesPeople.Dtos; using AWBlazorApp.Features.Sales.SalesPersonQuotaHistories.Dtos; using AWBlazorApp.Features.Sales.SalesReasons.Dtos; using AWBlazorApp.Features.Sales.SalesTaxRates.Dtos; using AWBlazorApp.Features.Sales.SalesTerritories.Dtos; using AWBlazorApp.Features.Sales.SalesTerritoryHistories.Dtos; using AWBlazorApp.Features.Sales.ShoppingCartItems.Dtos; using AWBlazorApp.Features.Sales.SpecialOffers.Dtos; using AWBlazorApp.Features.Sales.SpecialOfferProducts.Dtos; using AWBlazorApp.Features.Sales.Stores.Dtos; 
using FluentValidation;

namespace AWBlazorApp.Features.Sales.SalesOrderHeaderSalesReasons.Application.Validators;

public sealed class CreateSalesOrderHeaderSalesReasonValidator : AbstractValidator<CreateSalesOrderHeaderSalesReasonRequest>
{
    public CreateSalesOrderHeaderSalesReasonValidator()
    {
        RuleFor(x => x.SalesOrderId).GreaterThan(0).WithMessage("SalesOrderId is required.");
        RuleFor(x => x.SalesReasonId).GreaterThan(0).WithMessage("SalesReasonId is required.");
    }
}

public sealed class UpdateSalesOrderHeaderSalesReasonValidator : AbstractValidator<UpdateSalesOrderHeaderSalesReasonRequest>
{
    public UpdateSalesOrderHeaderSalesReasonValidator()
    {
        // No fields to validate — junction table.
    }
}
