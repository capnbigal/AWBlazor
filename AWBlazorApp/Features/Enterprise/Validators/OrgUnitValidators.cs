using AWBlazorApp.Features.Enterprise.Domain;
using AWBlazorApp.Features.Enterprise.Dtos;
using FluentValidation;

namespace AWBlazorApp.Features.Enterprise.Validators;

public sealed class CreateOrgUnitValidator : AbstractValidator<CreateOrgUnitRequest>
{
    public CreateOrgUnitValidator()
    {
        RuleFor(x => x.OrganizationId).GreaterThan(0);
        RuleFor(x => x.Kind).IsInEnum();
        RuleFor(x => x.Code).NotEmpty().MaximumLength(32);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
    }
}

public sealed class UpdateOrgUnitValidator : AbstractValidator<UpdateOrgUnitRequest>
{
    public UpdateOrgUnitValidator()
    {
        When(x => x.Kind.HasValue, () => RuleFor(x => x.Kind!.Value).IsInEnum());
        When(x => x.Code is not null, () =>
            RuleFor(x => x.Code!).NotEmpty().MaximumLength(32));
        When(x => x.Name is not null, () =>
            RuleFor(x => x.Name!).NotEmpty().MaximumLength(200));
    }
}
