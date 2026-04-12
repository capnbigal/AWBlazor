using ElementaryApp.Models.AdventureWorks;
using FluentValidation;

namespace ElementaryApp.Validators.AdventureWorks;

public sealed class CreateCustomerValidator : AbstractValidator<CreateCustomerRequest>
{
    public CreateCustomerValidator()
    {
        // A customer is either a person (PersonID) or a store (StoreID), exclusive.
        RuleFor(x => x).Must(x => (x.PersonId.HasValue ^ x.StoreId.HasValue))
            .WithMessage("Customer must be either a person (PersonId) or a store (StoreId), not both and not neither.");
        When(x => x.PersonId.HasValue, () =>
            RuleFor(x => x.PersonId!.Value).GreaterThan(0));
        When(x => x.StoreId.HasValue, () =>
            RuleFor(x => x.StoreId!.Value).GreaterThan(0));
        When(x => x.TerritoryId.HasValue, () =>
            RuleFor(x => x.TerritoryId!.Value).GreaterThan(0));
    }
}

public sealed class UpdateCustomerValidator : AbstractValidator<UpdateCustomerRequest>
{
    public UpdateCustomerValidator()
    {
        When(x => x.PersonId.HasValue, () => RuleFor(x => x.PersonId!.Value).GreaterThan(0));
        When(x => x.StoreId.HasValue, () => RuleFor(x => x.StoreId!.Value).GreaterThan(0));
        When(x => x.TerritoryId.HasValue, () => RuleFor(x => x.TerritoryId!.Value).GreaterThan(0));
    }
}
