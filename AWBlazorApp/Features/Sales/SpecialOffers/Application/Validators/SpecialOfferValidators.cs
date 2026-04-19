using AWBlazorApp.Features.Sales.CountryRegionCurrencies.Dtos; using AWBlazorApp.Features.Sales.CreditCards.Dtos; using AWBlazorApp.Features.Sales.Currencies.Dtos; using AWBlazorApp.Features.Sales.CurrencyRates.Dtos; using AWBlazorApp.Features.Sales.Customers.Dtos; using AWBlazorApp.Features.Sales.PersonCreditCards.Dtos; using AWBlazorApp.Features.Sales.SalesOrderDetails.Dtos; using AWBlazorApp.Features.Sales.SalesOrderHeaders.Dtos; using AWBlazorApp.Features.Sales.SalesOrderHeaderSalesReasons.Dtos; using AWBlazorApp.Features.Sales.SalesPeople.Dtos; using AWBlazorApp.Features.Sales.SalesPersonQuotaHistories.Dtos; using AWBlazorApp.Features.Sales.SalesReasons.Dtos; using AWBlazorApp.Features.Sales.SalesTaxRates.Dtos; using AWBlazorApp.Features.Sales.SalesTerritories.Dtos; using AWBlazorApp.Features.Sales.SalesTerritoryHistories.Dtos; using AWBlazorApp.Features.Sales.ShoppingCartItems.Dtos; using AWBlazorApp.Features.Sales.SpecialOffers.Dtos; using AWBlazorApp.Features.Sales.SpecialOfferProducts.Dtos; using AWBlazorApp.Features.Sales.Stores.Dtos; 
using FluentValidation;

namespace AWBlazorApp.Features.Sales.SpecialOffers.Application.Validators;

public sealed class CreateSpecialOfferValidator : AbstractValidator<CreateSpecialOfferRequest>
{
    public CreateSpecialOfferValidator()
    {
        RuleFor(x => x.Description).NotEmpty().WithMessage("Description is required.").MaximumLength(255);
        RuleFor(x => x.DiscountPct).InclusiveBetween(0m, 1m).WithMessage("Discount must be between 0.0 and 1.0.");
        RuleFor(x => x.OfferType).NotEmpty().WithMessage("Type is required.").MaximumLength(50);
        RuleFor(x => x.Category).NotEmpty().WithMessage("Category is required.").MaximumLength(50);
        RuleFor(x => x.MinQty).GreaterThanOrEqualTo(0).WithMessage("Min qty cannot be negative.");
        RuleFor(x => x.MaxQty!.Value).GreaterThanOrEqualTo(0).When(x => x.MaxQty.HasValue).WithMessage("Max qty cannot be negative.");
        RuleFor(x => x).Must(x => x.EndDate >= x.StartDate).WithMessage("End date must be on or after the start date.");
    }
}

public sealed class UpdateSpecialOfferValidator : AbstractValidator<UpdateSpecialOfferRequest>
{
    public UpdateSpecialOfferValidator()
    {
        When(x => x.Description is not null, () =>
            RuleFor(x => x.Description!).NotEmpty().MaximumLength(255));
        When(x => x.DiscountPct.HasValue, () =>
            RuleFor(x => x.DiscountPct!.Value).InclusiveBetween(0m, 1m));
        When(x => x.OfferType is not null, () =>
            RuleFor(x => x.OfferType!).NotEmpty().MaximumLength(50));
        When(x => x.Category is not null, () =>
            RuleFor(x => x.Category!).NotEmpty().MaximumLength(50));
        When(x => x.MinQty.HasValue, () =>
            RuleFor(x => x.MinQty!.Value).GreaterThanOrEqualTo(0));
        When(x => x.MaxQty.HasValue, () =>
            RuleFor(x => x.MaxQty!.Value).GreaterThanOrEqualTo(0));
        When(x => x.StartDate.HasValue && x.EndDate.HasValue, () =>
            RuleFor(x => x).Must(x => x.EndDate >= x.StartDate).WithMessage("End date must be on or after the start date."));
    }
}
