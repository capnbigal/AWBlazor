using AWBlazorApp.Features.Production.Cultures.Dtos; using AWBlazorApp.Features.Production.Documents.Dtos; using AWBlazorApp.Features.Production.Illustrations.Dtos; using AWBlazorApp.Features.Production.Locations.Dtos; using AWBlazorApp.Features.Production.ProductCategories.Dtos; using AWBlazorApp.Features.Production.ProductCostHistories.Dtos; using AWBlazorApp.Features.Production.ProductDescriptions.Dtos; using AWBlazorApp.Features.Production.ProductDocuments.Dtos; using AWBlazorApp.Features.Production.ProductInventories.Dtos; using AWBlazorApp.Features.Production.ProductListPriceHistories.Dtos; using AWBlazorApp.Features.Production.ProductModels.Dtos; using AWBlazorApp.Features.Production.ProductModelIllustrations.Dtos; using AWBlazorApp.Features.Production.ProductModelProductDescriptionCultures.Dtos; using AWBlazorApp.Features.Production.ProductPhotos.Dtos; using AWBlazorApp.Features.Production.ProductProductPhotos.Dtos; using AWBlazorApp.Features.Production.ProductReviews.Dtos; using AWBlazorApp.Features.Production.Products.Dtos; using AWBlazorApp.Features.Production.ProductSubcategories.Dtos; using AWBlazorApp.Features.Production.ScrapReasons.Dtos; using AWBlazorApp.Features.Production.TransactionHistories.Dtos; using AWBlazorApp.Features.Production.TransactionHistoryArchives.Dtos; using AWBlazorApp.Features.Production.UnitMeasures.Dtos; using AWBlazorApp.Features.Production.WorkOrders.Dtos; using AWBlazorApp.Features.Production.WorkOrderRoutings.Dtos; 
using FluentValidation;

namespace AWBlazorApp.Features.Production.ProductPhotos.Application.Validators;

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
