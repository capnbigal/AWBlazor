using AWBlazorApp.Features.Person.Dtos;
using FluentValidation;

namespace AWBlazorApp.Features.Person.Validators;

public sealed class CreateContactTypeValidator : AbstractValidator<CreateContactTypeRequest>
{
    public CreateContactTypeValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required.").MaximumLength(50);
    }
}

public sealed class UpdateContactTypeValidator : AbstractValidator<UpdateContactTypeRequest>
{
    public UpdateContactTypeValidator()
    {
        When(x => x.Name is not null, () =>
        {
            RuleFor(x => x.Name!).NotEmpty().WithMessage("Name cannot be blanked out.").MaximumLength(50);
        });
    }
}
