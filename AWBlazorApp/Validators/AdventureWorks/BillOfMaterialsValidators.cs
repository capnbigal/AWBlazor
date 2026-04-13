using AWBlazorApp.Models.AdventureWorks;
using FluentValidation;

namespace AWBlazorApp.Validators.AdventureWorks;

public sealed class CreateBillOfMaterialsValidator : AbstractValidator<CreateBillOfMaterialsRequest>
{
    public CreateBillOfMaterialsValidator()
    {
        RuleFor(x => x.ComponentId).GreaterThan(0).WithMessage("ComponentId is required.");
        RuleFor(x => x.UnitMeasureCode)
            .NotEmpty().WithMessage("UnitMeasureCode is required.")
            .MaximumLength(3);
        RuleFor(x => x.BomLevel).GreaterThanOrEqualTo((short)0);
        RuleFor(x => x.PerAssemblyQty).GreaterThan(0).WithMessage("PerAssemblyQty must be positive.");
        When(x => x.ProductAssemblyId.HasValue, () =>
            RuleFor(x => x.ProductAssemblyId!.Value).GreaterThan(0));
        When(x => x.EndDate.HasValue, () =>
            RuleFor(x => x.EndDate!.Value).GreaterThanOrEqualTo(x => x.StartDate)
                .WithMessage("EndDate cannot be earlier than StartDate."));
    }
}

public sealed class UpdateBillOfMaterialsValidator : AbstractValidator<UpdateBillOfMaterialsRequest>
{
    public UpdateBillOfMaterialsValidator()
    {
        When(x => x.ComponentId.HasValue, () => RuleFor(x => x.ComponentId!.Value).GreaterThan(0));
        When(x => x.ProductAssemblyId.HasValue, () => RuleFor(x => x.ProductAssemblyId!.Value).GreaterThan(0));
        When(x => x.UnitMeasureCode is not null, () => RuleFor(x => x.UnitMeasureCode!).NotEmpty().MaximumLength(3));
        When(x => x.BomLevel.HasValue, () => RuleFor(x => x.BomLevel!.Value).GreaterThanOrEqualTo((short)0));
        When(x => x.PerAssemblyQty.HasValue, () => RuleFor(x => x.PerAssemblyQty!.Value).GreaterThan(0));
    }
}
