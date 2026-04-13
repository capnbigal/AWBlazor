using AWBlazorApp.Models.AdventureWorks;
using FluentValidation;

namespace AWBlazorApp.Validators.AdventureWorks;

public sealed class CreateSalesPersonValidator : AbstractValidator<CreateSalesPersonRequest>
{
    public CreateSalesPersonValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("BusinessEntityId is required (must already exist in Person.BusinessEntity).");
        RuleFor(x => x.Bonus).GreaterThanOrEqualTo(0).WithMessage("Bonus cannot be negative.");
        RuleFor(x => x.CommissionPct).InclusiveBetween(0m, 1m).WithMessage("Commission must be between 0.0 and 1.0.");
        When(x => x.SalesQuota.HasValue, () =>
            RuleFor(x => x.SalesQuota!.Value).GreaterThanOrEqualTo(0));
        When(x => x.TerritoryId.HasValue, () =>
            RuleFor(x => x.TerritoryId!.Value).GreaterThan(0));
    }
}

public sealed class UpdateSalesPersonValidator : AbstractValidator<UpdateSalesPersonRequest>
{
    public UpdateSalesPersonValidator()
    {
        When(x => x.Bonus.HasValue, () => RuleFor(x => x.Bonus!.Value).GreaterThanOrEqualTo(0));
        When(x => x.CommissionPct.HasValue, () => RuleFor(x => x.CommissionPct!.Value).InclusiveBetween(0m, 1m));
        When(x => x.SalesQuota.HasValue, () => RuleFor(x => x.SalesQuota!.Value).GreaterThanOrEqualTo(0));
        When(x => x.TerritoryId.HasValue, () => RuleFor(x => x.TerritoryId!.Value).GreaterThan(0));
    }
}
