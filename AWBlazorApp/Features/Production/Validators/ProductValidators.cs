using AWBlazorApp.Features.Production.Dtos;
using FluentValidation;

namespace AWBlazorApp.Features.Production.Validators;

public sealed class CreateProductValidator : AbstractValidator<CreateProductRequest>
{
    /// <summary>SQL CHECK: Class is one of H (High), M (Medium), L (Low).</summary>
    private static readonly string[] AllowedClasses = ["H", "M", "L"];

    /// <summary>SQL CHECK: Style is one of W (Womens), M (Mens), U (Universal).</summary>
    private static readonly string[] AllowedStyles = ["W", "M", "U"];

    /// <summary>SQL CHECK: ProductLine is one of R (Road), M (Mountain), T (Touring), S (Standard).</summary>
    private static readonly string[] AllowedProductLines = ["R", "M", "T", "S"];

    public CreateProductValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(50);
        RuleFor(x => x.ProductNumber).NotEmpty().MaximumLength(25);
        RuleFor(x => x.SafetyStockLevel).GreaterThan((short)0);
        RuleFor(x => x.ReorderPoint).GreaterThan((short)0);
        RuleFor(x => x.StandardCost).GreaterThanOrEqualTo(0);
        RuleFor(x => x.ListPrice).GreaterThanOrEqualTo(0);
        RuleFor(x => x.DaysToManufacture).GreaterThanOrEqualTo(0);
        RuleFor(x => x.SellStartDate).NotEmpty();

        When(x => !string.IsNullOrEmpty(x.Color), () => RuleFor(x => x.Color!).MaximumLength(15));
        When(x => !string.IsNullOrEmpty(x.Size), () => RuleFor(x => x.Size!).MaximumLength(5));
        When(x => !string.IsNullOrEmpty(x.SizeUnitMeasureCode), () => RuleFor(x => x.SizeUnitMeasureCode!).Length(3));
        When(x => !string.IsNullOrEmpty(x.WeightUnitMeasureCode), () => RuleFor(x => x.WeightUnitMeasureCode!).Length(3));

        When(x => !string.IsNullOrEmpty(x.ProductLine), () =>
            RuleFor(x => x.ProductLine!).Must(p => AllowedProductLines.Contains(p))
                .WithMessage("ProductLine must be R (Road), M (Mountain), T (Touring), or S (Standard)."));
        When(x => !string.IsNullOrEmpty(x.Class), () =>
            RuleFor(x => x.Class!).Must(c => AllowedClasses.Contains(c))
                .WithMessage("Class must be H (High), M (Medium), or L (Low)."));
        When(x => !string.IsNullOrEmpty(x.Style), () =>
            RuleFor(x => x.Style!).Must(s => AllowedStyles.Contains(s))
                .WithMessage("Style must be W (Womens), M (Mens), or U (Universal)."));

        When(x => x.ProductSubcategoryId.HasValue, () => RuleFor(x => x.ProductSubcategoryId!.Value).GreaterThan(0));
        When(x => x.ProductModelId.HasValue, () => RuleFor(x => x.ProductModelId!.Value).GreaterThan(0));
        When(x => x.Weight.HasValue, () => RuleFor(x => x.Weight!.Value).GreaterThan(0));

        When(x => x.SellEndDate.HasValue, () =>
            RuleFor(x => x.SellEndDate!.Value).GreaterThanOrEqualTo(x => x.SellStartDate)
                .WithMessage("SellEndDate cannot be before SellStartDate."));
    }
}

public sealed class UpdateProductValidator : AbstractValidator<UpdateProductRequest>
{
    private static readonly string[] AllowedClasses = ["H", "M", "L"];
    private static readonly string[] AllowedStyles = ["W", "M", "U"];
    private static readonly string[] AllowedProductLines = ["R", "M", "T", "S"];

    public UpdateProductValidator()
    {
        When(x => x.Name is not null, () => RuleFor(x => x.Name!).NotEmpty().MaximumLength(50));
        When(x => x.ProductNumber is not null, () => RuleFor(x => x.ProductNumber!).NotEmpty().MaximumLength(25));
        When(x => x.SafetyStockLevel.HasValue, () => RuleFor(x => x.SafetyStockLevel!.Value).GreaterThan((short)0));
        When(x => x.ReorderPoint.HasValue, () => RuleFor(x => x.ReorderPoint!.Value).GreaterThan((short)0));
        When(x => x.StandardCost.HasValue, () => RuleFor(x => x.StandardCost!.Value).GreaterThanOrEqualTo(0));
        When(x => x.ListPrice.HasValue, () => RuleFor(x => x.ListPrice!.Value).GreaterThanOrEqualTo(0));
        When(x => x.DaysToManufacture.HasValue, () => RuleFor(x => x.DaysToManufacture!.Value).GreaterThanOrEqualTo(0));
        When(x => !string.IsNullOrEmpty(x.Color), () => RuleFor(x => x.Color!).MaximumLength(15));
        When(x => !string.IsNullOrEmpty(x.Size), () => RuleFor(x => x.Size!).MaximumLength(5));
        When(x => !string.IsNullOrEmpty(x.SizeUnitMeasureCode), () => RuleFor(x => x.SizeUnitMeasureCode!).Length(3));
        When(x => !string.IsNullOrEmpty(x.WeightUnitMeasureCode), () => RuleFor(x => x.WeightUnitMeasureCode!).Length(3));
        When(x => !string.IsNullOrEmpty(x.ProductLine), () =>
            RuleFor(x => x.ProductLine!).Must(p => AllowedProductLines.Contains(p))
                .WithMessage("ProductLine must be R, M, T, or S."));
        When(x => !string.IsNullOrEmpty(x.Class), () =>
            RuleFor(x => x.Class!).Must(c => AllowedClasses.Contains(c))
                .WithMessage("Class must be H, M, or L."));
        When(x => !string.IsNullOrEmpty(x.Style), () =>
            RuleFor(x => x.Style!).Must(s => AllowedStyles.Contains(s))
                .WithMessage("Style must be W, M, or U."));
        When(x => x.ProductSubcategoryId.HasValue, () => RuleFor(x => x.ProductSubcategoryId!.Value).GreaterThan(0));
        When(x => x.ProductModelId.HasValue, () => RuleFor(x => x.ProductModelId!.Value).GreaterThan(0));
        When(x => x.Weight.HasValue, () => RuleFor(x => x.Weight!.Value).GreaterThan(0));
    }
}
