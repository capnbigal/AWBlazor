using AWBlazorApp.Features.Production.Dtos; using AWBlazorApp.Features.Production.Cultures.Dtos; using AWBlazorApp.Features.Production.Documents.Dtos; using AWBlazorApp.Features.Production.Illustrations.Dtos; using AWBlazorApp.Features.Production.Locations.Dtos; using AWBlazorApp.Features.Production.ProductCategories.Dtos; using AWBlazorApp.Features.Production.ProductCostHistories.Dtos; using AWBlazorApp.Features.Production.ProductDescriptions.Dtos; using AWBlazorApp.Features.Production.ProductDocuments.Dtos; using AWBlazorApp.Features.Production.ProductInventories.Dtos; using AWBlazorApp.Features.Production.ProductListPriceHistories.Dtos; using AWBlazorApp.Features.Production.ProductModels.Dtos; using AWBlazorApp.Features.Production.ProductModelIllustrations.Dtos; using AWBlazorApp.Features.Production.ProductModelProductDescriptionCultures.Dtos; using AWBlazorApp.Features.Production.ProductPhotos.Dtos; using AWBlazorApp.Features.Production.ProductProductPhotos.Dtos; using AWBlazorApp.Features.Production.ProductReviews.Dtos; using AWBlazorApp.Features.Production.Products.Dtos; using AWBlazorApp.Features.Production.ProductSubcategories.Dtos; using AWBlazorApp.Features.Production.ScrapReasons.Dtos; using AWBlazorApp.Features.Production.TransactionHistories.Dtos; using AWBlazorApp.Features.Production.TransactionHistoryArchives.Dtos; using AWBlazorApp.Features.Production.UnitMeasures.Dtos; using AWBlazorApp.Features.Production.WorkOrders.Dtos; using AWBlazorApp.Features.Production.WorkOrderRoutings.Dtos; 
using FluentValidation;

namespace AWBlazorApp.Features.Production.WorkOrders.Application.Validators;

public sealed class CreateWorkOrderValidator : AbstractValidator<CreateWorkOrderRequest>
{
    public CreateWorkOrderValidator()
    {
        RuleFor(x => x.ProductId).GreaterThan(0).WithMessage("ProductId is required.");
        RuleFor(x => x.OrderQty).GreaterThan(0).WithMessage("OrderQty must be greater than zero.");
        RuleFor(x => x.ScrappedQty).GreaterThanOrEqualTo((short)0);
        RuleFor(x => x.DueDate).GreaterThanOrEqualTo(x => x.StartDate)
            .WithMessage("DueDate cannot be earlier than StartDate.");
        When(x => x.EndDate.HasValue, () =>
            RuleFor(x => x.EndDate!.Value).GreaterThanOrEqualTo(x => x.StartDate)
                .WithMessage("EndDate cannot be earlier than StartDate."));
        When(x => x.ScrapReasonId.HasValue, () =>
            RuleFor(x => x.ScrapReasonId!.Value).GreaterThan((short)0));
    }
}

public sealed class UpdateWorkOrderValidator : AbstractValidator<UpdateWorkOrderRequest>
{
    public UpdateWorkOrderValidator()
    {
        When(x => x.ProductId.HasValue, () => RuleFor(x => x.ProductId!.Value).GreaterThan(0));
        When(x => x.OrderQty.HasValue, () => RuleFor(x => x.OrderQty!.Value).GreaterThan(0));
        When(x => x.ScrappedQty.HasValue, () => RuleFor(x => x.ScrappedQty!.Value).GreaterThanOrEqualTo((short)0));
        When(x => x.ScrapReasonId.HasValue, () => RuleFor(x => x.ScrapReasonId!.Value).GreaterThan((short)0));
    }
}
