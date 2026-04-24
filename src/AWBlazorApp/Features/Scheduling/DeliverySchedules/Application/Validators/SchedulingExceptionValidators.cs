using AWBlazorApp.Features.Scheduling.DeliverySchedules.Dtos;
using FluentValidation;

namespace AWBlazorApp.Features.Scheduling.DeliverySchedules.Application.Validators;

public sealed class CreateSchedulingExceptionValidator : AbstractValidator<CreateSchedulingExceptionRequest>
{
    public CreateSchedulingExceptionValidator()
    {
        RuleFor(x => x.WeekId).GreaterThan(0);
        RuleFor(x => x.LocationId).GreaterThan((short)0);
        RuleFor(x => x.SalesOrderDetailId).GreaterThan(0);
        RuleFor(x => x.ExceptionType).InclusiveBetween((byte)1, (byte)3)
            .WithMessage("ExceptionType must be 1 (ManualSequencePin), 2 (KittingHold), or 3 (HotOrderBump).");
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
        When(x => x.ExceptionType == 1, () =>
            RuleFor(x => x.PinnedSequence).NotNull().GreaterThan(0)
                .WithMessage("PinnedSequence is required and must be positive for ManualSequencePin."));
    }
}
