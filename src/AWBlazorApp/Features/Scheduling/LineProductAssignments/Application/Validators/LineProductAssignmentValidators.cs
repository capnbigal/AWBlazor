using AWBlazorApp.Features.Scheduling.LineProductAssignments.Dtos;
using FluentValidation;

namespace AWBlazorApp.Features.Scheduling.LineProductAssignments.Application.Validators;

public sealed class CreateLineProductAssignmentValidator : AbstractValidator<CreateLineProductAssignmentRequest>
{
    public CreateLineProductAssignmentValidator()
    {
        RuleFor(x => x.LocationId).GreaterThan((short)0);
        RuleFor(x => x.ProductModelId).GreaterThan(0);
    }
}

public sealed class UpdateLineProductAssignmentValidator : AbstractValidator<UpdateLineProductAssignmentRequest>
{
    public UpdateLineProductAssignmentValidator() { /* only IsActive; no constraints */ }
}
