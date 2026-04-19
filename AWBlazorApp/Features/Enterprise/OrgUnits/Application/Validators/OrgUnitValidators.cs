using AWBlazorApp.Features.Enterprise.Assets.Domain; using AWBlazorApp.Features.Enterprise.CostCenters.Domain; using AWBlazorApp.Features.Enterprise.OrgUnits.Domain; using AWBlazorApp.Features.Enterprise.Organizations.Domain; using AWBlazorApp.Features.Enterprise.ProductLines.Domain; using AWBlazorApp.Features.Enterprise.Stations.Domain; 
using AWBlazorApp.Features.Enterprise.Assets.Dtos; using AWBlazorApp.Features.Enterprise.CostCenters.Dtos; using AWBlazorApp.Features.Enterprise.OrgUnits.Dtos; using AWBlazorApp.Features.Enterprise.Organizations.Dtos; using AWBlazorApp.Features.Enterprise.ProductLines.Dtos; using AWBlazorApp.Features.Enterprise.Stations.Dtos; 
using FluentValidation;

namespace AWBlazorApp.Features.Enterprise.OrgUnits.Application.Validators;

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
