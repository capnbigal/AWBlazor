using AWBlazorApp.Features.Enterprise.Assets.Dtos; using AWBlazorApp.Features.Enterprise.CostCenters.Dtos; using AWBlazorApp.Features.Enterprise.OrgUnits.Dtos; using AWBlazorApp.Features.Enterprise.Organizations.Dtos; using AWBlazorApp.Features.Enterprise.ProductLines.Dtos; using AWBlazorApp.Features.Enterprise.Stations.Dtos; 
using FluentValidation;

namespace AWBlazorApp.Features.Enterprise.CostCenters.Application.Validators;

public sealed class CreateCostCenterValidator : AbstractValidator<CreateCostCenterRequest>
{
    public CreateCostCenterValidator()
    {
        RuleFor(x => x.OrganizationId).GreaterThan(0);
        RuleFor(x => x.Code).NotEmpty().MaximumLength(32);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
    }
}

public sealed class UpdateCostCenterValidator : AbstractValidator<UpdateCostCenterRequest>
{
    public UpdateCostCenterValidator()
    {
        When(x => x.Code is not null, () => RuleFor(x => x.Code!).NotEmpty().MaximumLength(32));
        When(x => x.Name is not null, () => RuleFor(x => x.Name!).NotEmpty().MaximumLength(200));
    }
}
