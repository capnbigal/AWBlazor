using AWBlazorApp.Features.Production.Dtos; using AWBlazorApp.Features.Production.Cultures.Dtos; using AWBlazorApp.Features.Production.Documents.Dtos; using AWBlazorApp.Features.Production.Illustrations.Dtos; using AWBlazorApp.Features.Production.Locations.Dtos; using AWBlazorApp.Features.Production.ProductCategories.Dtos; using AWBlazorApp.Features.Production.ProductCostHistories.Dtos; using AWBlazorApp.Features.Production.ProductDescriptions.Dtos; using AWBlazorApp.Features.Production.ProductDocuments.Dtos; using AWBlazorApp.Features.Production.ProductInventories.Dtos; using AWBlazorApp.Features.Production.ProductListPriceHistories.Dtos; using AWBlazorApp.Features.Production.ProductModels.Dtos; using AWBlazorApp.Features.Production.ProductModelIllustrations.Dtos; using AWBlazorApp.Features.Production.ProductModelProductDescriptionCultures.Dtos; using AWBlazorApp.Features.Production.ProductPhotos.Dtos; using AWBlazorApp.Features.Production.ProductProductPhotos.Dtos; using AWBlazorApp.Features.Production.ProductReviews.Dtos; using AWBlazorApp.Features.Production.Products.Dtos; using AWBlazorApp.Features.Production.ProductSubcategories.Dtos; using AWBlazorApp.Features.Production.ScrapReasons.Dtos; using AWBlazorApp.Features.Production.TransactionHistories.Dtos; using AWBlazorApp.Features.Production.TransactionHistoryArchives.Dtos; using AWBlazorApp.Features.Production.UnitMeasures.Dtos; using AWBlazorApp.Features.Production.WorkOrders.Dtos; using AWBlazorApp.Features.Production.WorkOrderRoutings.Dtos; 
using FluentValidation;

namespace AWBlazorApp.Features.Production.TransactionHistoryArchives.Application.Validators;

public sealed class CreateTransactionHistoryArchiveValidator : AbstractValidator<CreateTransactionHistoryArchiveRequest>
{
    private static readonly string[] ValidTransactionTypes = ["W", "S", "P"];

    public CreateTransactionHistoryArchiveValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("Id (TransactionID) is required.");
        RuleFor(x => x.ProductId).GreaterThan(0).WithMessage("ProductId is required.");
        RuleFor(x => x.ReferenceOrderId).GreaterThan(0).WithMessage("ReferenceOrderId is required.");
        RuleFor(x => x.TransactionType).NotEmpty().MaximumLength(1)
            .Must(t => ValidTransactionTypes.Contains(t))
            .WithMessage("TransactionType must be W, S, or P.");
        RuleFor(x => x.ActualCost).GreaterThanOrEqualTo(0);
    }
}

public sealed class UpdateTransactionHistoryArchiveValidator : AbstractValidator<UpdateTransactionHistoryArchiveRequest>
{
    private static readonly string[] ValidTransactionTypes = ["W", "S", "P"];

    public UpdateTransactionHistoryArchiveValidator()
    {
        When(x => x.ProductId.HasValue, () => RuleFor(x => x.ProductId!.Value).GreaterThan(0));
        When(x => x.ReferenceOrderId.HasValue, () => RuleFor(x => x.ReferenceOrderId!.Value).GreaterThan(0));
        When(x => x.TransactionType is not null, () =>
            RuleFor(x => x.TransactionType!).NotEmpty().MaximumLength(1)
                .Must(t => ValidTransactionTypes.Contains(t))
                .WithMessage("TransactionType must be W, S, or P."));
        When(x => x.ActualCost.HasValue, () => RuleFor(x => x.ActualCost!.Value).GreaterThanOrEqualTo(0));
    }
}
