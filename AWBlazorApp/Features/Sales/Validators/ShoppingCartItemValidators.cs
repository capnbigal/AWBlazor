using AWBlazorApp.Features.Sales.Models;
using FluentValidation;

namespace AWBlazorApp.Features.Sales.Validators;

public sealed class CreateShoppingCartItemValidator : AbstractValidator<CreateShoppingCartItemRequest>
{
    public CreateShoppingCartItemValidator()
    {
        RuleFor(x => x.ShoppingCartId).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Quantity).GreaterThan(0).WithMessage("Quantity must be positive.");
        RuleFor(x => x.ProductId).GreaterThan(0).WithMessage("Product id is required.");
    }
}

public sealed class UpdateShoppingCartItemValidator : AbstractValidator<UpdateShoppingCartItemRequest>
{
    public UpdateShoppingCartItemValidator()
    {
        When(x => x.ShoppingCartId is not null, () => RuleFor(x => x.ShoppingCartId!).NotEmpty().MaximumLength(50));
        When(x => x.Quantity.HasValue, () => RuleFor(x => x.Quantity!.Value).GreaterThan(0));
        When(x => x.ProductId.HasValue, () => RuleFor(x => x.ProductId!.Value).GreaterThan(0));
    }
}
