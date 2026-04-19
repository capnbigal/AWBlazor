using AWBlazorApp.Features.Sales.Dtos;
using FluentValidation;

namespace AWBlazorApp.Features.Sales.Validators;

public sealed class CreateStoreValidator : AbstractValidator<CreateStoreRequest>
{
    public CreateStoreValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("BusinessEntityId is required.");
        RuleFor(x => x.Name).NotEmpty().MaximumLength(50);
        When(x => x.SalesPersonId.HasValue, () =>
            RuleFor(x => x.SalesPersonId!.Value).GreaterThan(0));
    }
}

public sealed class UpdateStoreValidator : AbstractValidator<UpdateStoreRequest>
{
    public UpdateStoreValidator()
    {
        When(x => x.Name is not null, () => RuleFor(x => x.Name!).NotEmpty().MaximumLength(50));
        When(x => x.SalesPersonId.HasValue, () => RuleFor(x => x.SalesPersonId!.Value).GreaterThan(0));
    }
}
