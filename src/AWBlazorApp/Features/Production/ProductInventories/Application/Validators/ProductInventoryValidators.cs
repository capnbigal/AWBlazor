using AWBlazorApp.Features.Production.Cultures.Dtos; using AWBlazorApp.Features.Production.Documents.Dtos; using AWBlazorApp.Features.Production.Illustrations.Dtos; using AWBlazorApp.Features.Production.Locations.Dtos; using AWBlazorApp.Features.Production.ProductCategories.Dtos; using AWBlazorApp.Features.Production.ProductCostHistories.Dtos; using AWBlazorApp.Features.Production.ProductDescriptions.Dtos; using AWBlazorApp.Features.Production.ProductDocuments.Dtos; using AWBlazorApp.Features.Production.ProductInventories.Dtos; using AWBlazorApp.Features.Production.ProductListPriceHistories.Dtos; using AWBlazorApp.Features.Production.ProductModels.Dtos; using AWBlazorApp.Features.Production.ProductModelIllustrations.Dtos; using AWBlazorApp.Features.Production.ProductModelProductDescriptionCultures.Dtos; using AWBlazorApp.Features.Production.ProductPhotos.Dtos; using AWBlazorApp.Features.Production.ProductProductPhotos.Dtos; using AWBlazorApp.Features.Production.ProductReviews.Dtos; using AWBlazorApp.Features.Production.Products.Dtos; using AWBlazorApp.Features.Production.ProductSubcategories.Dtos; using AWBlazorApp.Features.Production.ScrapReasons.Dtos; using AWBlazorApp.Features.Production.TransactionHistories.Dtos; using AWBlazorApp.Features.Production.TransactionHistoryArchives.Dtos; using AWBlazorApp.Features.Production.UnitMeasures.Dtos; using AWBlazorApp.Features.Production.WorkOrders.Dtos; using AWBlazorApp.Features.Production.WorkOrderRoutings.Dtos; 
using FluentValidation;

namespace AWBlazorApp.Features.Production.ProductInventories.Application.Validators;

public sealed class CreateProductInventoryValidator : AbstractValidator<CreateProductInventoryRequest>
{
    public CreateProductInventoryValidator()
    {
        RuleFor(x => x.ProductId).GreaterThan(0).WithMessage("ProductId is required.");
        RuleFor(x => x.LocationId).GreaterThan((short)0).WithMessage("LocationId is required.");
        RuleFor(x => x.Shelf).NotEmpty().MaximumLength(10);
        RuleFor(x => x.Bin).LessThanOrEqualTo((byte)100)
            .WithMessage("Bin must be between 0 and 100 (SQL CHECK constraint).");
        RuleFor(x => x.Quantity).GreaterThanOrEqualTo((short)0);
    }
}

public sealed class UpdateProductInventoryValidator : AbstractValidator<UpdateProductInventoryRequest>
{
    public UpdateProductInventoryValidator()
    {
        When(x => x.Shelf is not null, () => RuleFor(x => x.Shelf!).NotEmpty().MaximumLength(10));
        When(x => x.Bin.HasValue, () => RuleFor(x => x.Bin!.Value).LessThanOrEqualTo((byte)100));
        When(x => x.Quantity.HasValue, () => RuleFor(x => x.Quantity!.Value).GreaterThanOrEqualTo((short)0));
    }
}
