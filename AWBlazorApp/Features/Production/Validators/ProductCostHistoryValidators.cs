using AWBlazorApp.Features.Production.Dtos;
using FluentValidation;

namespace AWBlazorApp.Features.Production.Validators;

public sealed class CreateProductCostHistoryValidator : AbstractValidator<CreateProductCostHistoryRequest>
{
    public CreateProductCostHistoryValidator()
    {
        RuleFor(x => x.ProductId).GreaterThan(0).WithMessage("ProductId is required.");
        RuleFor(x => x.StartDate).NotEmpty();
        RuleFor(x => x.StandardCost).GreaterThanOrEqualTo(0);
        When(x => x.EndDate.HasValue, () =>
            RuleFor(x => x.EndDate!.Value).GreaterThan(x => x.StartDate)
                .WithMessage("EndDate must be after StartDate."));
    }
}

public sealed class UpdateProductCostHistoryValidator : AbstractValidator<UpdateProductCostHistoryRequest>
{
    public UpdateProductCostHistoryValidator()
    {
        When(x => x.StandardCost.HasValue, () =>
            RuleFor(x => x.StandardCost!.Value).GreaterThanOrEqualTo(0));
    }
}
