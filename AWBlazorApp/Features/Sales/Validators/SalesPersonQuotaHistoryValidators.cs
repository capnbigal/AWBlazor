using AWBlazorApp.Features.Sales.Dtos;
using FluentValidation;

namespace AWBlazorApp.Features.Sales.Validators;

public sealed class CreateSalesPersonQuotaHistoryValidator : AbstractValidator<CreateSalesPersonQuotaHistoryRequest>
{
    public CreateSalesPersonQuotaHistoryValidator()
    {
        RuleFor(x => x.BusinessEntityId).GreaterThan(0).WithMessage("BusinessEntityId is required.");
        RuleFor(x => x.QuotaDate).NotEmpty().WithMessage("QuotaDate is required.");
        RuleFor(x => x.SalesQuota).GreaterThanOrEqualTo(0).WithMessage("SalesQuota cannot be negative.");
    }
}

public sealed class UpdateSalesPersonQuotaHistoryValidator : AbstractValidator<UpdateSalesPersonQuotaHistoryRequest>
{
    public UpdateSalesPersonQuotaHistoryValidator()
    {
        When(x => x.SalesQuota.HasValue, () =>
            RuleFor(x => x.SalesQuota!.Value).GreaterThanOrEqualTo(0));
    }
}
