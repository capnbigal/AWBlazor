using ElementaryApp.Models.AdventureWorks;
using FluentValidation;

namespace ElementaryApp.Validators.AdventureWorks;

public sealed class CreateProductPhotoValidator : AbstractValidator<CreateProductPhotoRequest>
{
    public CreateProductPhotoValidator()
    {
        When(x => !string.IsNullOrEmpty(x.ThumbnailPhotoFileName), () =>
            RuleFor(x => x.ThumbnailPhotoFileName!).MaximumLength(50));
        When(x => !string.IsNullOrEmpty(x.LargePhotoFileName), () =>
            RuleFor(x => x.LargePhotoFileName!).MaximumLength(50));
    }
}

public sealed class UpdateProductPhotoValidator : AbstractValidator<UpdateProductPhotoRequest>
{
    public UpdateProductPhotoValidator()
    {
        When(x => !string.IsNullOrEmpty(x.ThumbnailPhotoFileName), () =>
            RuleFor(x => x.ThumbnailPhotoFileName!).MaximumLength(50));
        When(x => !string.IsNullOrEmpty(x.LargePhotoFileName), () =>
            RuleFor(x => x.LargePhotoFileName!).MaximumLength(50));
    }
}
