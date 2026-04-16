using AWBlazorApp.Features.Sales.Models;
using FluentValidation;

namespace AWBlazorApp.Features.Sales.Validators;

public sealed class CreateSalesOrderHeaderSalesReasonValidator : AbstractValidator<CreateSalesOrderHeaderSalesReasonRequest>
{
    public CreateSalesOrderHeaderSalesReasonValidator()
    {
        RuleFor(x => x.SalesOrderId).GreaterThan(0).WithMessage("SalesOrderId is required.");
        RuleFor(x => x.SalesReasonId).GreaterThan(0).WithMessage("SalesReasonId is required.");
    }
}

public sealed class UpdateSalesOrderHeaderSalesReasonValidator : AbstractValidator<UpdateSalesOrderHeaderSalesReasonRequest>
{
    public UpdateSalesOrderHeaderSalesReasonValidator()
    {
        // No fields to validate — junction table.
    }
}
