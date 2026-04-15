using AWBlazorApp.Features.AdventureWorks.Models;
using FluentValidation;

namespace AWBlazorApp.Features.AdventureWorks.Validators;

public sealed class CreateProductListPriceHistoryValidator : AbstractValidator<CreateProductListPriceHistoryRequest>
{
    public CreateProductListPriceHistoryValidator()
    {
        RuleFor(x => x.ProductId).GreaterThan(0).WithMessage("ProductId is required.");
        RuleFor(x => x.StartDate).NotEmpty();
        RuleFor(x => x.ListPrice).GreaterThanOrEqualTo(0);
        When(x => x.EndDate.HasValue, () =>
            RuleFor(x => x.EndDate!.Value).GreaterThan(x => x.StartDate)
                .WithMessage("EndDate must be after StartDate."));
    }
}

public sealed class UpdateProductListPriceHistoryValidator : AbstractValidator<UpdateProductListPriceHistoryRequest>
{
    public UpdateProductListPriceHistoryValidator()
    {
        When(x => x.ListPrice.HasValue, () =>
            RuleFor(x => x.ListPrice!.Value).GreaterThanOrEqualTo(0));
    }
}
