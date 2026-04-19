using AWBlazorApp.Features.Production.Dtos; using AWBlazorApp.Features.Production.Cultures.Dtos; using AWBlazorApp.Features.Production.Documents.Dtos; using AWBlazorApp.Features.Production.Illustrations.Dtos; using AWBlazorApp.Features.Production.Locations.Dtos; using AWBlazorApp.Features.Production.ProductCategories.Dtos; using AWBlazorApp.Features.Production.ProductCostHistories.Dtos; using AWBlazorApp.Features.Production.ProductDescriptions.Dtos; using AWBlazorApp.Features.Production.ProductDocuments.Dtos; using AWBlazorApp.Features.Production.ProductInventories.Dtos; using AWBlazorApp.Features.Production.ProductListPriceHistories.Dtos; using AWBlazorApp.Features.Production.ProductModels.Dtos; using AWBlazorApp.Features.Production.ProductModelIllustrations.Dtos; using AWBlazorApp.Features.Production.ProductModelProductDescriptionCultures.Dtos; using AWBlazorApp.Features.Production.ProductPhotos.Dtos; using AWBlazorApp.Features.Production.ProductProductPhotos.Dtos; using AWBlazorApp.Features.Production.ProductReviews.Dtos; using AWBlazorApp.Features.Production.Products.Dtos; using AWBlazorApp.Features.Production.ProductSubcategories.Dtos; using AWBlazorApp.Features.Production.ScrapReasons.Dtos; using AWBlazorApp.Features.Production.TransactionHistories.Dtos; using AWBlazorApp.Features.Production.TransactionHistoryArchives.Dtos; using AWBlazorApp.Features.Production.UnitMeasures.Dtos; using AWBlazorApp.Features.Production.WorkOrders.Dtos; using AWBlazorApp.Features.Production.WorkOrderRoutings.Dtos; 
using FluentValidation;

namespace AWBlazorApp.Features.Production.ProductReviews.Application.Validators;

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
