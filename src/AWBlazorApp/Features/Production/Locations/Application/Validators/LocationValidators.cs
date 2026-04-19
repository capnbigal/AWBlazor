using AWBlazorApp.Features.Production.Cultures.Dtos; using AWBlazorApp.Features.Production.Documents.Dtos; using AWBlazorApp.Features.Production.Illustrations.Dtos; using AWBlazorApp.Features.Production.Locations.Dtos; using AWBlazorApp.Features.Production.ProductCategories.Dtos; using AWBlazorApp.Features.Production.ProductCostHistories.Dtos; using AWBlazorApp.Features.Production.ProductDescriptions.Dtos; using AWBlazorApp.Features.Production.ProductDocuments.Dtos; using AWBlazorApp.Features.Production.ProductInventories.Dtos; using AWBlazorApp.Features.Production.ProductListPriceHistories.Dtos; using AWBlazorApp.Features.Production.ProductModels.Dtos; using AWBlazorApp.Features.Production.ProductModelIllustrations.Dtos; using AWBlazorApp.Features.Production.ProductModelProductDescriptionCultures.Dtos; using AWBlazorApp.Features.Production.ProductPhotos.Dtos; using AWBlazorApp.Features.Production.ProductProductPhotos.Dtos; using AWBlazorApp.Features.Production.ProductReviews.Dtos; using AWBlazorApp.Features.Production.Products.Dtos; using AWBlazorApp.Features.Production.ProductSubcategories.Dtos; using AWBlazorApp.Features.Production.ScrapReasons.Dtos; using AWBlazorApp.Features.Production.TransactionHistories.Dtos; using AWBlazorApp.Features.Production.TransactionHistoryArchives.Dtos; using AWBlazorApp.Features.Production.UnitMeasures.Dtos; using AWBlazorApp.Features.Production.WorkOrders.Dtos; using AWBlazorApp.Features.Production.WorkOrderRoutings.Dtos; 
using FluentValidation;

namespace AWBlazorApp.Features.Production.Locations.Application.Validators;

public sealed class CreateLocationValidator : AbstractValidator<CreateLocationRequest>
{
    public CreateLocationValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required.").MaximumLength(50);
        RuleFor(x => x.CostRate).GreaterThanOrEqualTo(0).WithMessage("Cost rate cannot be negative.");
        RuleFor(x => x.Availability).GreaterThanOrEqualTo(0).WithMessage("Availability cannot be negative.");
    }
}

public sealed class UpdateLocationValidator : AbstractValidator<UpdateLocationRequest>
{
    public UpdateLocationValidator()
    {
        When(x => x.Name is not null, () =>
        {
            RuleFor(x => x.Name!).NotEmpty().WithMessage("Name cannot be blanked out.").MaximumLength(50);
        });
        When(x => x.CostRate.HasValue, () =>
        {
            RuleFor(x => x.CostRate!.Value).GreaterThanOrEqualTo(0).WithMessage("Cost rate cannot be negative.");
        });
        When(x => x.Availability.HasValue, () =>
        {
            RuleFor(x => x.Availability!.Value).GreaterThanOrEqualTo(0).WithMessage("Availability cannot be negative.");
        });
    }
}
