using AWBlazorApp.Features.Production.Models;
using FluentValidation;

namespace AWBlazorApp.Features.Production.Validators;

public sealed class CreateProductInventoryValidator : AbstractValidator<CreateProductInventoryRequest>
{
    public CreateProductInventoryValidator()
    {
        RuleFor(x => x.ProductId).GreaterThan(0).WithMessage("ProductId is required.");
        RuleFor(x => x.LocationId).GreaterThan((short)0).WithMessage("LocationId is required.");
        RuleFor(x => x.Shelf).NotEmpty().MaximumLength(10);
        RuleFor(x => x.Bin).LessThanOrEqualTo((byte)100)
            .WithMessage("Bin must be between 0 and 100 (SQL CHECK constraint).");
        RuleFor(x => x.Quantity).GreaterThanOrEqualTo((short)0);
    }
}

public sealed class UpdateProductInventoryValidator : AbstractValidator<UpdateProductInventoryRequest>
{
    public UpdateProductInventoryValidator()
    {
        When(x => x.Shelf is not null, () => RuleFor(x => x.Shelf!).NotEmpty().MaximumLength(10));
        When(x => x.Bin.HasValue, () => RuleFor(x => x.Bin!.Value).LessThanOrEqualTo((byte)100));
        When(x => x.Quantity.HasValue, () => RuleFor(x => x.Quantity!.Value).GreaterThanOrEqualTo((short)0));
    }
}
