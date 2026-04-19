using AWBlazorApp.Features.Enterprise.Assets.Domain; using AWBlazorApp.Features.Enterprise.CostCenters.Domain; using AWBlazorApp.Features.Enterprise.OrgUnits.Domain; using AWBlazorApp.Features.Enterprise.Organizations.Domain; using AWBlazorApp.Features.Enterprise.ProductLines.Domain; using AWBlazorApp.Features.Enterprise.Stations.Domain; 
using AWBlazorApp.Features.Enterprise.Assets.Dtos; using AWBlazorApp.Features.Enterprise.CostCenters.Dtos; using AWBlazorApp.Features.Enterprise.OrgUnits.Dtos; using AWBlazorApp.Features.Enterprise.Organizations.Dtos; using AWBlazorApp.Features.Enterprise.ProductLines.Dtos; using AWBlazorApp.Features.Enterprise.Stations.Dtos; 
using FluentValidation;

namespace AWBlazorApp.Features.Enterprise.Stations.Application.Validators;

public sealed class CreateStationValidator : AbstractValidator<CreateStationRequest>
{
    public CreateStationValidator()
    {
        RuleFor(x => x.OrgUnitId).GreaterThan(0);
        RuleFor(x => x.StationKind).IsInEnum();
        RuleFor(x => x.Code).NotEmpty().MaximumLength(32);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        When(x => x.IdealCycleSeconds.HasValue,
            () => RuleFor(x => x.IdealCycleSeconds!.Value).GreaterThan(0)
                .WithMessage("Ideal cycle seconds must be greater than 0."));
    }
}

public sealed class UpdateStationValidator : AbstractValidator<UpdateStationRequest>
{
    public UpdateStationValidator()
    {
        When(x => x.OrgUnitId.HasValue, () => RuleFor(x => x.OrgUnitId!.Value).GreaterThan(0));
        When(x => x.StationKind.HasValue, () => RuleFor(x => x.StationKind!.Value).IsInEnum());
        When(x => x.Code is not null, () => RuleFor(x => x.Code!).NotEmpty().MaximumLength(32));
        When(x => x.Name is not null, () => RuleFor(x => x.Name!).NotEmpty().MaximumLength(200));
        When(x => x.IdealCycleSeconds.HasValue,
            () => RuleFor(x => x.IdealCycleSeconds!.Value).GreaterThan(0));
    }
}
