using AWBlazorApp.Models.AdventureWorks;
using FluentValidation;

namespace AWBlazorApp.Validators.AdventureWorks;

public sealed class CreateProductVendorValidator : AbstractValidator<CreateProductVendorRequest>
{
    public CreateProductVendorValidator()
    {
        RuleFor(x => x.ProductId).GreaterThan(0).WithMessage("ProductId is required.");
        RuleFor(x => x.BusinessEntityId).GreaterThan(0).WithMessage("BusinessEntityId (vendor) is required.");
        RuleFor(x => x.AverageLeadTime).GreaterThan(0).WithMessage("AverageLeadTime must be greater than zero.");
        RuleFor(x => x.StandardPrice).GreaterThanOrEqualTo(0).WithMessage("StandardPrice cannot be negative.");
        RuleFor(x => x.MinOrderQty).GreaterThan(0).WithMessage("MinOrderQty must be greater than zero.");
        RuleFor(x => x.MaxOrderQty).GreaterThanOrEqualTo(x => x.MinOrderQty).WithMessage("MaxOrderQty must be >= MinOrderQty.");
        RuleFor(x => x.UnitMeasureCode).NotEmpty().WithMessage("UnitMeasureCode is required.").MaximumLength(3);
        When(x => x.LastReceiptCost.HasValue, () =>
            RuleFor(x => x.LastReceiptCost!.Value).GreaterThanOrEqualTo(0));
        When(x => x.OnOrderQty.HasValue, () =>
            RuleFor(x => x.OnOrderQty!.Value).GreaterThanOrEqualTo(0));
    }
}

public sealed class UpdateProductVendorValidator : AbstractValidator<UpdateProductVendorRequest>
{
    public UpdateProductVendorValidator()
    {
        When(x => x.AverageLeadTime.HasValue, () =>
            RuleFor(x => x.AverageLeadTime!.Value).GreaterThan(0));
        When(x => x.StandardPrice.HasValue, () =>
            RuleFor(x => x.StandardPrice!.Value).GreaterThanOrEqualTo(0));
        When(x => x.MinOrderQty.HasValue, () =>
            RuleFor(x => x.MinOrderQty!.Value).GreaterThan(0));
        When(x => x.MaxOrderQty.HasValue, () =>
            RuleFor(x => x.MaxOrderQty!.Value).GreaterThan(0));
        When(x => x.UnitMeasureCode is not null, () =>
            RuleFor(x => x.UnitMeasureCode!).NotEmpty().MaximumLength(3));
        When(x => x.LastReceiptCost.HasValue, () =>
            RuleFor(x => x.LastReceiptCost!.Value).GreaterThanOrEqualTo(0));
        When(x => x.OnOrderQty.HasValue, () =>
            RuleFor(x => x.OnOrderQty!.Value).GreaterThanOrEqualTo(0));
    }
}
