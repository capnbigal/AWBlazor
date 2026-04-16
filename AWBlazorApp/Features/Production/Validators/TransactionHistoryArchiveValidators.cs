using AWBlazorApp.Features.Production.Models;
using FluentValidation;

namespace AWBlazorApp.Features.Production.Validators;

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
