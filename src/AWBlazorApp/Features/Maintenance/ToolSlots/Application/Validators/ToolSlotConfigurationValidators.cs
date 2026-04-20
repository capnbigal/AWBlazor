using AWBlazorApp.Shared.Dtos;
using AWBlazorApp.Features.Maintenance.ToolSlots.Dtos;
using FluentValidation;

namespace AWBlazorApp.Shared.Validation;

public sealed class CreateToolSlotConfigurationValidator : AbstractValidator<CreateToolSlotConfigurationRequest>
{
    public CreateToolSlotConfigurationValidator()
    {
        // The original ServiceStack DTO had no required fields, so we only enforce the soft
        // constraint that at least the family/code/destination tuple is populated.
        RuleFor(x => x)
            .Must(x => !string.IsNullOrWhiteSpace(x.Family)
                        || !string.IsNullOrWhiteSpace(x.MtCode)
                        || !string.IsNullOrWhiteSpace(x.Destination))
            .WithMessage("At least one of Family, MtCode, or Destination must be set.");
    }
}

public sealed class UpdateToolSlotConfigurationValidator : AbstractValidator<UpdateToolSlotConfigurationRequest>
{
    // No field-level constraints — every property is optional and patches a single column.
}
