using AWBlazorApp.Features.Scheduling.LineConfigurations.Dtos;
using FluentValidation;

namespace AWBlazorApp.Features.Scheduling.LineConfigurations.Application.Validators;

public sealed class CreateLineConfigurationValidator : AbstractValidator<CreateLineConfigurationRequest>
{
    public CreateLineConfigurationValidator()
    {
        RuleFor(x => x.LocationId).GreaterThan((short)0).WithMessage("LocationId must reference a Production.Location row.");
        RuleFor(x => x.TaktSeconds).GreaterThan(0).WithMessage("TaktSeconds must be positive.");
        RuleFor(x => x.ShiftsPerDay).InclusiveBetween((byte)1, (byte)3).WithMessage("ShiftsPerDay must be 1, 2, or 3.");
        RuleFor(x => x.MinutesPerShift).GreaterThan((short)0).LessThanOrEqualTo((short)720).WithMessage("MinutesPerShift must be between 1 and 720.");
        RuleFor(x => x.FrozenLookaheadHours).GreaterThanOrEqualTo(0).WithMessage("FrozenLookaheadHours cannot be negative.");
    }
}

public sealed class UpdateLineConfigurationValidator : AbstractValidator<UpdateLineConfigurationRequest>
{
    public UpdateLineConfigurationValidator()
    {
        When(x => x.TaktSeconds.HasValue, () =>
            RuleFor(x => x.TaktSeconds!.Value).GreaterThan(0));
        When(x => x.ShiftsPerDay.HasValue, () =>
            RuleFor(x => x.ShiftsPerDay!.Value).InclusiveBetween((byte)1, (byte)3));
        When(x => x.MinutesPerShift.HasValue, () =>
            RuleFor(x => x.MinutesPerShift!.Value).GreaterThan((short)0).LessThanOrEqualTo((short)720));
        When(x => x.FrozenLookaheadHours.HasValue, () =>
            RuleFor(x => x.FrozenLookaheadHours!.Value).GreaterThanOrEqualTo(0));
    }
}
