using AWBlazorApp.Features.Sales.CountryRegionCurrencies.Dtos; using AWBlazorApp.Features.Sales.CreditCards.Dtos; using AWBlazorApp.Features.Sales.Currencies.Dtos; using AWBlazorApp.Features.Sales.CurrencyRates.Dtos; using AWBlazorApp.Features.Sales.Customers.Dtos; using AWBlazorApp.Features.Sales.PersonCreditCards.Dtos; using AWBlazorApp.Features.Sales.SalesOrderDetails.Dtos; using AWBlazorApp.Features.Sales.SalesOrderHeaders.Dtos; using AWBlazorApp.Features.Sales.SalesOrderHeaderSalesReasons.Dtos; using AWBlazorApp.Features.Sales.SalesPeople.Dtos; using AWBlazorApp.Features.Sales.SalesPersonQuotaHistories.Dtos; using AWBlazorApp.Features.Sales.SalesReasons.Dtos; using AWBlazorApp.Features.Sales.SalesTaxRates.Dtos; using AWBlazorApp.Features.Sales.SalesTerritories.Dtos; using AWBlazorApp.Features.Sales.SalesTerritoryHistories.Dtos; using AWBlazorApp.Features.Sales.ShoppingCartItems.Dtos; using AWBlazorApp.Features.Sales.SpecialOffers.Dtos; using AWBlazorApp.Features.Sales.SpecialOfferProducts.Dtos; using AWBlazorApp.Features.Sales.Stores.Dtos; 
using FluentValidation;

namespace AWBlazorApp.Features.Sales.SalesOrderDetails.Application.Validators;

public sealed class CreateSalesOrderDetailValidator : AbstractValidator<CreateSalesOrderDetailRequest>
{
    public CreateSalesOrderDetailValidator()
    {
        RuleFor(x => x.SalesOrderId).GreaterThan(0).WithMessage("SalesOrderId is required.");
        RuleFor(x => x.OrderQty).GreaterThan((short)0).WithMessage("OrderQty must be at least 1.");
        RuleFor(x => x.ProductId).GreaterThan(0).WithMessage("ProductId is required.");
        RuleFor(x => x.SpecialOfferId).GreaterThan(0).WithMessage("SpecialOfferId is required.");
        RuleFor(x => x.UnitPrice).GreaterThanOrEqualTo(0);
        RuleFor(x => x.UnitPriceDiscount).GreaterThanOrEqualTo(0);
        When(x => x.CarrierTrackingNumber is not null, () => RuleFor(x => x.CarrierTrackingNumber!).MaximumLength(25));
    }
}

public sealed class UpdateSalesOrderDetailValidator : AbstractValidator<UpdateSalesOrderDetailRequest>
{
    public UpdateSalesOrderDetailValidator()
    {
        When(x => x.OrderQty.HasValue, () => RuleFor(x => x.OrderQty!.Value).GreaterThan((short)0));
        When(x => x.UnitPrice.HasValue, () => RuleFor(x => x.UnitPrice!.Value).GreaterThanOrEqualTo(0));
        When(x => x.UnitPriceDiscount.HasValue, () => RuleFor(x => x.UnitPriceDiscount!.Value).GreaterThanOrEqualTo(0));
        When(x => x.CarrierTrackingNumber is not null, () => RuleFor(x => x.CarrierTrackingNumber!).MaximumLength(25));
    }
}
