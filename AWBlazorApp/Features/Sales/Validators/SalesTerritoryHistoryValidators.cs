using AWBlazorApp.Features.Sales.Dtos;
using FluentValidation;

namespace AWBlazorApp.Features.Sales.Validators;

public sealed class CreateSalesTerritoryHistoryValidator : AbstractValidator<CreateSalesTerritoryHistoryRequest>
{
    public CreateSalesTerritoryHistoryValidator()
    {
        RuleFor(x => x.BusinessEntityId).GreaterThan(0).WithMessage("BusinessEntityId is required.");
        RuleFor(x => x.TerritoryId).GreaterThan(0).WithMessage("TerritoryId is required.");
        RuleFor(x => x.StartDate).NotEmpty();
        When(x => x.EndDate.HasValue, () =>
            RuleFor(x => x.EndDate!.Value).GreaterThan(x => x.StartDate)
                .WithMessage("EndDate must be after StartDate."));
    }
}

public sealed class UpdateSalesTerritoryHistoryValidator : AbstractValidator<UpdateSalesTerritoryHistoryRequest>
{
    public UpdateSalesTerritoryHistoryValidator()
    {
        // EndDate is the only updatable field — no further validation needed.
    }
}
