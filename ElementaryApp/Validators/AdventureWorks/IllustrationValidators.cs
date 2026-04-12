using ElementaryApp.Models.AdventureWorks;
using FluentValidation;

namespace ElementaryApp.Validators.AdventureWorks;

public sealed class CreateIllustrationValidator : AbstractValidator<CreateIllustrationRequest>
{
    public CreateIllustrationValidator() { }
}

public sealed class UpdateIllustrationValidator : AbstractValidator<UpdateIllustrationRequest>
{
    public UpdateIllustrationValidator() { }
}
