using ElementaryApp.Models.AdventureWorks;
using FluentValidation;

namespace ElementaryApp.Validators.AdventureWorks;

public sealed class CreatePhoneNumberTypeValidator : AbstractValidator<CreatePhoneNumberTypeRequest>
{
    public CreatePhoneNumberTypeValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required.").MaximumLength(50);
    }
}

public sealed class UpdatePhoneNumberTypeValidator : AbstractValidator<UpdatePhoneNumberTypeRequest>
{
    public UpdatePhoneNumberTypeValidator()
    {
        When(x => x.Name is not null, () =>
        {
            RuleFor(x => x.Name!).NotEmpty().WithMessage("Name cannot be blanked out.").MaximumLength(50);
        });
    }
}
