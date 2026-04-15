using AWBlazorApp.Features.AdventureWorks.Models;
using FluentValidation;

namespace AWBlazorApp.Features.AdventureWorks.Validators;

public sealed class CreateIllustrationValidator : AbstractValidator<CreateIllustrationRequest>
{
    public CreateIllustrationValidator() { }
}

public sealed class UpdateIllustrationValidator : AbstractValidator<UpdateIllustrationRequest>
{
    public UpdateIllustrationValidator() { }
}
