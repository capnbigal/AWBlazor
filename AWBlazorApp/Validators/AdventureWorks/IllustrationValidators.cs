using AWBlazorApp.Models.AdventureWorks;
using FluentValidation;

namespace AWBlazorApp.Validators.AdventureWorks;

public sealed class CreateIllustrationValidator : AbstractValidator<CreateIllustrationRequest>
{
    public CreateIllustrationValidator() { }
}

public sealed class UpdateIllustrationValidator : AbstractValidator<UpdateIllustrationRequest>
{
    public UpdateIllustrationValidator() { }
}
