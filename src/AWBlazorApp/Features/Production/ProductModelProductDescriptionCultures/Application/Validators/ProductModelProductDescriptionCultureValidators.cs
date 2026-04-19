using AWBlazorApp.Features.Production.Cultures.Dtos; using AWBlazorApp.Features.Production.Documents.Dtos; using AWBlazorApp.Features.Production.Illustrations.Dtos; using AWBlazorApp.Features.Production.Locations.Dtos; using AWBlazorApp.Features.Production.ProductCategories.Dtos; using AWBlazorApp.Features.Production.ProductCostHistories.Dtos; using AWBlazorApp.Features.Production.ProductDescriptions.Dtos; using AWBlazorApp.Features.Production.ProductDocuments.Dtos; using AWBlazorApp.Features.Production.ProductInventories.Dtos; using AWBlazorApp.Features.Production.ProductListPriceHistories.Dtos; using AWBlazorApp.Features.Production.ProductModels.Dtos; using AWBlazorApp.Features.Production.ProductModelIllustrations.Dtos; using AWBlazorApp.Features.Production.ProductModelProductDescriptionCultures.Dtos; using AWBlazorApp.Features.Production.ProductPhotos.Dtos; using AWBlazorApp.Features.Production.ProductProductPhotos.Dtos; using AWBlazorApp.Features.Production.ProductReviews.Dtos; using AWBlazorApp.Features.Production.Products.Dtos; using AWBlazorApp.Features.Production.ProductSubcategories.Dtos; using AWBlazorApp.Features.Production.ScrapReasons.Dtos; using AWBlazorApp.Features.Production.TransactionHistories.Dtos; using AWBlazorApp.Features.Production.TransactionHistoryArchives.Dtos; using AWBlazorApp.Features.Production.UnitMeasures.Dtos; using AWBlazorApp.Features.Production.WorkOrders.Dtos; using AWBlazorApp.Features.Production.WorkOrderRoutings.Dtos; 
using FluentValidation;

namespace AWBlazorApp.Features.Production.ProductModelProductDescriptionCultures.Application.Validators;

public sealed class CreateProductModelProductDescriptionCultureValidator : AbstractValidator<CreateProductModelProductDescriptionCultureRequest>
{
    public CreateProductModelProductDescriptionCultureValidator()
    {
        RuleFor(x => x.ProductModelId).GreaterThan(0).WithMessage("ProductModelId is required.");
        RuleFor(x => x.ProductDescriptionId).GreaterThan(0).WithMessage("ProductDescriptionId is required.");
        RuleFor(x => x.CultureId).NotEmpty().MaximumLength(6).WithMessage("CultureId is required (max 6 chars).");
    }
}

public sealed class UpdateProductModelProductDescriptionCultureValidator : AbstractValidator<UpdateProductModelProductDescriptionCultureRequest>
{
    public UpdateProductModelProductDescriptionCultureValidator()
    {
        // No fields to validate — junction table.
    }
}
