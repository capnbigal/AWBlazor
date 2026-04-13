using AWBlazorApp.Models.AdventureWorks;
using FluentValidation;

namespace AWBlazorApp.Validators.AdventureWorks;

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
