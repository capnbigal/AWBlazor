using ElementaryApp.Models.AdventureWorks;
using FluentValidation;

namespace ElementaryApp.Validators.AdventureWorks;

public sealed class CreateStateProvinceValidator : AbstractValidator<CreateStateProvinceRequest>
{
    public CreateStateProvinceValidator()
    {
        RuleFor(x => x.StateProvinceCode).NotEmpty().MaximumLength(3);
        RuleFor(x => x.CountryRegionCode).NotEmpty().MaximumLength(3);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(50);
        RuleFor(x => x.TerritoryId).GreaterThan(0).WithMessage("Territory id is required.");
    }
}

public sealed class UpdateStateProvinceValidator : AbstractValidator<UpdateStateProvinceRequest>
{
    public UpdateStateProvinceValidator()
    {
        When(x => x.StateProvinceCode is not null, () =>
            RuleFor(x => x.StateProvinceCode!).NotEmpty().MaximumLength(3));
        When(x => x.CountryRegionCode is not null, () =>
            RuleFor(x => x.CountryRegionCode!).NotEmpty().MaximumLength(3));
        When(x => x.Name is not null, () =>
            RuleFor(x => x.Name!).NotEmpty().MaximumLength(50));
        When(x => x.TerritoryId.HasValue, () =>
            RuleFor(x => x.TerritoryId!.Value).GreaterThan(0));
    }
}
