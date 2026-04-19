using AWBlazorApp.Features.Production.Dtos; using AWBlazorApp.Features.Production.Cultures.Dtos; using AWBlazorApp.Features.Production.Documents.Dtos; using AWBlazorApp.Features.Production.Illustrations.Dtos; using AWBlazorApp.Features.Production.Locations.Dtos; using AWBlazorApp.Features.Production.ProductCategories.Dtos; using AWBlazorApp.Features.Production.ProductCostHistories.Dtos; using AWBlazorApp.Features.Production.ProductDescriptions.Dtos; using AWBlazorApp.Features.Production.ProductDocuments.Dtos; using AWBlazorApp.Features.Production.ProductInventories.Dtos; using AWBlazorApp.Features.Production.ProductListPriceHistories.Dtos; using AWBlazorApp.Features.Production.ProductModels.Dtos; using AWBlazorApp.Features.Production.ProductModelIllustrations.Dtos; using AWBlazorApp.Features.Production.ProductModelProductDescriptionCultures.Dtos; using AWBlazorApp.Features.Production.ProductPhotos.Dtos; using AWBlazorApp.Features.Production.ProductProductPhotos.Dtos; using AWBlazorApp.Features.Production.ProductReviews.Dtos; using AWBlazorApp.Features.Production.Products.Dtos; using AWBlazorApp.Features.Production.ProductSubcategories.Dtos; using AWBlazorApp.Features.Production.ScrapReasons.Dtos; using AWBlazorApp.Features.Production.TransactionHistories.Dtos; using AWBlazorApp.Features.Production.TransactionHistoryArchives.Dtos; using AWBlazorApp.Features.Production.UnitMeasures.Dtos; using AWBlazorApp.Features.Production.WorkOrders.Dtos; using AWBlazorApp.Features.Production.WorkOrderRoutings.Dtos; 
using FluentValidation;

namespace AWBlazorApp.Features.Production.Validators;

public sealed class CreateBillOfMaterialsValidator : AbstractValidator<CreateBillOfMaterialsRequest>
{
    public CreateBillOfMaterialsValidator()
    {
        RuleFor(x => x.ComponentId).GreaterThan(0).WithMessage("ComponentId is required.");
        RuleFor(x => x.UnitMeasureCode)
            .NotEmpty().WithMessage("UnitMeasureCode is required.")
            .MaximumLength(3);
        RuleFor(x => x.BomLevel).GreaterThanOrEqualTo((short)0);
        RuleFor(x => x.PerAssemblyQty).GreaterThan(0).WithMessage("PerAssemblyQty must be positive.");
        When(x => x.ProductAssemblyId.HasValue, () =>
            RuleFor(x => x.ProductAssemblyId!.Value).GreaterThan(0));
        When(x => x.EndDate.HasValue, () =>
            RuleFor(x => x.EndDate!.Value).GreaterThanOrEqualTo(x => x.StartDate)
                .WithMessage("EndDate cannot be earlier than StartDate."));
    }
}

public sealed class UpdateBillOfMaterialsValidator : AbstractValidator<UpdateBillOfMaterialsRequest>
{
    public UpdateBillOfMaterialsValidator()
    {
        When(x => x.ComponentId.HasValue, () => RuleFor(x => x.ComponentId!.Value).GreaterThan(0));
        When(x => x.ProductAssemblyId.HasValue, () => RuleFor(x => x.ProductAssemblyId!.Value).GreaterThan(0));
        When(x => x.UnitMeasureCode is not null, () => RuleFor(x => x.UnitMeasureCode!).NotEmpty().MaximumLength(3));
        When(x => x.BomLevel.HasValue, () => RuleFor(x => x.BomLevel!.Value).GreaterThanOrEqualTo((short)0));
        When(x => x.PerAssemblyQty.HasValue, () => RuleFor(x => x.PerAssemblyQty!.Value).GreaterThan(0));
    }
}
