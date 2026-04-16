using AWBlazorApp.Features.Production.Models;
using FluentValidation;

namespace AWBlazorApp.Features.Production.Validators;

public sealed class CreateProductReviewValidator : AbstractValidator<CreateProductReviewRequest>
{
    public CreateProductReviewValidator()
    {
        RuleFor(x => x.ProductId).GreaterThan(0).WithMessage("ProductId is required.");
        RuleFor(x => x.ReviewerName).NotEmpty().MaximumLength(50);
        RuleFor(x => x.EmailAddress).NotEmpty().MaximumLength(50).EmailAddress();
        RuleFor(x => x.Rating).InclusiveBetween(1, 5).WithMessage("Rating must be 1–5.");
        When(x => !string.IsNullOrEmpty(x.Comments), () =>
            RuleFor(x => x.Comments!).MaximumLength(3850));
    }
}

public sealed class UpdateProductReviewValidator : AbstractValidator<UpdateProductReviewRequest>
{
    public UpdateProductReviewValidator()
    {
        When(x => x.ProductId.HasValue, () => RuleFor(x => x.ProductId!.Value).GreaterThan(0));
        When(x => x.ReviewerName is not null, () => RuleFor(x => x.ReviewerName!).NotEmpty().MaximumLength(50));
        When(x => x.EmailAddress is not null, () => RuleFor(x => x.EmailAddress!).NotEmpty().MaximumLength(50).EmailAddress());
        When(x => x.Rating.HasValue, () => RuleFor(x => x.Rating!.Value).InclusiveBetween(1, 5));
        When(x => !string.IsNullOrEmpty(x.Comments), () => RuleFor(x => x.Comments!).MaximumLength(3850));
    }
}
