using AWBlazorApp.Features.Production.Dtos;
using FluentValidation;

namespace AWBlazorApp.Features.Production.Validators;

public sealed class CreateWorkOrderRoutingValidator : AbstractValidator<CreateWorkOrderRoutingRequest>
{
    public CreateWorkOrderRoutingValidator()
    {
        RuleFor(x => x.WorkOrderId).GreaterThan(0).WithMessage("WorkOrderId is required.");
        RuleFor(x => x.ProductId).GreaterThan(0).WithMessage("ProductId is required.");
        RuleFor(x => x.OperationSequence).GreaterThan((short)0).WithMessage("OperationSequence must be > 0.");
        RuleFor(x => x.LocationId).GreaterThan((short)0).WithMessage("LocationId is required.");
        RuleFor(x => x.PlannedCost).GreaterThanOrEqualTo(0);
        RuleFor(x => x.ScheduledEndDate).GreaterThanOrEqualTo(x => x.ScheduledStartDate)
            .WithMessage("ScheduledEndDate cannot be earlier than ScheduledStartDate.");
        When(x => x.ActualResourceHrs.HasValue, () =>
            RuleFor(x => x.ActualResourceHrs!.Value).GreaterThanOrEqualTo(0));
        When(x => x.ActualCost.HasValue, () =>
            RuleFor(x => x.ActualCost!.Value).GreaterThanOrEqualTo(0));
    }
}

public sealed class UpdateWorkOrderRoutingValidator : AbstractValidator<UpdateWorkOrderRoutingRequest>
{
    public UpdateWorkOrderRoutingValidator()
    {
        When(x => x.LocationId.HasValue, () => RuleFor(x => x.LocationId!.Value).GreaterThan((short)0));
        When(x => x.PlannedCost.HasValue, () => RuleFor(x => x.PlannedCost!.Value).GreaterThanOrEqualTo(0));
        When(x => x.ActualResourceHrs.HasValue, () => RuleFor(x => x.ActualResourceHrs!.Value).GreaterThanOrEqualTo(0));
        When(x => x.ActualCost.HasValue, () => RuleFor(x => x.ActualCost!.Value).GreaterThanOrEqualTo(0));
    }
}
