using AWBlazorApp.Features.Production.Models;
using FluentValidation;

namespace AWBlazorApp.Features.Production.Validators;

public sealed class CreateWorkOrderValidator : AbstractValidator<CreateWorkOrderRequest>
{
    public CreateWorkOrderValidator()
    {
        RuleFor(x => x.ProductId).GreaterThan(0).WithMessage("ProductId is required.");
        RuleFor(x => x.OrderQty).GreaterThan(0).WithMessage("OrderQty must be greater than zero.");
        RuleFor(x => x.ScrappedQty).GreaterThanOrEqualTo((short)0);
        RuleFor(x => x.DueDate).GreaterThanOrEqualTo(x => x.StartDate)
            .WithMessage("DueDate cannot be earlier than StartDate.");
        When(x => x.EndDate.HasValue, () =>
            RuleFor(x => x.EndDate!.Value).GreaterThanOrEqualTo(x => x.StartDate)
                .WithMessage("EndDate cannot be earlier than StartDate."));
        When(x => x.ScrapReasonId.HasValue, () =>
            RuleFor(x => x.ScrapReasonId!.Value).GreaterThan((short)0));
    }
}

public sealed class UpdateWorkOrderValidator : AbstractValidator<UpdateWorkOrderRequest>
{
    public UpdateWorkOrderValidator()
    {
        When(x => x.ProductId.HasValue, () => RuleFor(x => x.ProductId!.Value).GreaterThan(0));
        When(x => x.OrderQty.HasValue, () => RuleFor(x => x.OrderQty!.Value).GreaterThan(0));
        When(x => x.ScrappedQty.HasValue, () => RuleFor(x => x.ScrappedQty!.Value).GreaterThanOrEqualTo((short)0));
        When(x => x.ScrapReasonId.HasValue, () => RuleFor(x => x.ScrapReasonId!.Value).GreaterThan((short)0));
    }
}
