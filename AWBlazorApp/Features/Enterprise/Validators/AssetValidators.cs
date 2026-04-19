using AWBlazorApp.Features.Enterprise.Domain;
using AWBlazorApp.Features.Enterprise.Dtos;
using FluentValidation;

namespace AWBlazorApp.Features.Enterprise.Validators;

public sealed class CreateAssetValidator : AbstractValidator<CreateAssetRequest>
{
    public CreateAssetValidator()
    {
        RuleFor(x => x.OrganizationId).GreaterThan(0);
        RuleFor(x => x.AssetTag).NotEmpty().MaximumLength(64);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.AssetType).IsInEnum();
        RuleFor(x => x.Status).IsInEnum();
        RuleFor(x => x.Manufacturer).MaximumLength(128);
        RuleFor(x => x.Model).MaximumLength(128);
        RuleFor(x => x.SerialNumber).MaximumLength(128);
        RuleFor(x => x)
            .Must(x => !x.DecommissionedAt.HasValue || !x.CommissionedAt.HasValue
                      || x.DecommissionedAt.Value >= x.CommissionedAt.Value)
            .WithMessage("DecommissionedAt must be on or after CommissionedAt.");
    }
}

public sealed class UpdateAssetValidator : AbstractValidator<UpdateAssetRequest>
{
    public UpdateAssetValidator()
    {
        When(x => x.AssetTag is not null, () => RuleFor(x => x.AssetTag!).NotEmpty().MaximumLength(64));
        When(x => x.Name is not null, () => RuleFor(x => x.Name!).NotEmpty().MaximumLength(200));
        When(x => x.AssetType.HasValue, () => RuleFor(x => x.AssetType!.Value).IsInEnum());
        When(x => x.Status.HasValue, () => RuleFor(x => x.Status!.Value).IsInEnum());
        When(x => x.Manufacturer is not null, () => RuleFor(x => x.Manufacturer!).MaximumLength(128));
        When(x => x.Model is not null, () => RuleFor(x => x.Model!).MaximumLength(128));
        When(x => x.SerialNumber is not null, () => RuleFor(x => x.SerialNumber!).MaximumLength(128));
    }
}
