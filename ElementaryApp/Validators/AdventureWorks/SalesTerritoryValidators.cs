using ElementaryApp.Models.AdventureWorks;
using FluentValidation;

namespace ElementaryApp.Validators.AdventureWorks;

public sealed class CreateSalesTerritoryValidator : AbstractValidator<CreateSalesTerritoryRequest>
{
    public CreateSalesTerritoryValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required.").MaximumLength(50);
        RuleFor(x => x.CountryRegionCode).NotEmpty().WithMessage("Country/region code is required.").MaximumLength(3);
        RuleFor(x => x.GroupName).NotEmpty().WithMessage("Group is required.").MaximumLength(50);
    }
}

public sealed class UpdateSalesTerritoryValidator : AbstractValidator<UpdateSalesTerritoryRequest>
{
    public UpdateSalesTerritoryValidator()
    {
        When(x => x.Name is not null, () => RuleFor(x => x.Name!).NotEmpty().MaximumLength(50));
        When(x => x.CountryRegionCode is not null, () => RuleFor(x => x.CountryRegionCode!).NotEmpty().MaximumLength(3));
        When(x => x.GroupName is not null, () => RuleFor(x => x.GroupName!).NotEmpty().MaximumLength(50));
    }
}
