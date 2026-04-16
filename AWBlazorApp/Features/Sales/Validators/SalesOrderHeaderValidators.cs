using AWBlazorApp.Features.Sales.Models;
using FluentValidation;

namespace AWBlazorApp.Features.Sales.Validators;

public sealed class CreateSalesOrderHeaderValidator : AbstractValidator<CreateSalesOrderHeaderRequest>
{
    public CreateSalesOrderHeaderValidator()
    {
        RuleFor(x => x.OrderDate).NotEmpty();
        RuleFor(x => x.DueDate).NotEmpty();
        RuleFor(x => x.Status).InclusiveBetween((byte)1, (byte)6).WithMessage("Status must be 1–6.");
        RuleFor(x => x.CustomerId).GreaterThan(0).WithMessage("CustomerId is required.");
        RuleFor(x => x.BillToAddressId).GreaterThan(0).WithMessage("BillToAddressId is required.");
        RuleFor(x => x.ShipToAddressId).GreaterThan(0).WithMessage("ShipToAddressId is required.");
        RuleFor(x => x.ShipMethodId).GreaterThan(0).WithMessage("ShipMethodId is required.");
        RuleFor(x => x.SubTotal).GreaterThanOrEqualTo(0);
        RuleFor(x => x.TaxAmt).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Freight).GreaterThanOrEqualTo(0);
        When(x => x.PurchaseOrderNumber is not null, () => RuleFor(x => x.PurchaseOrderNumber!).MaximumLength(25));
        When(x => x.AccountNumber is not null, () => RuleFor(x => x.AccountNumber!).MaximumLength(15));
        When(x => x.CreditCardApprovalCode is not null, () => RuleFor(x => x.CreditCardApprovalCode!).MaximumLength(15));
        When(x => x.Comment is not null, () => RuleFor(x => x.Comment!).MaximumLength(128));
    }
}

public sealed class UpdateSalesOrderHeaderValidator : AbstractValidator<UpdateSalesOrderHeaderRequest>
{
    public UpdateSalesOrderHeaderValidator()
    {
        When(x => x.Status.HasValue, () => RuleFor(x => x.Status!.Value).InclusiveBetween((byte)1, (byte)6));
        When(x => x.ShipMethodId.HasValue, () => RuleFor(x => x.ShipMethodId!.Value).GreaterThan(0));
        When(x => x.SubTotal.HasValue, () => RuleFor(x => x.SubTotal!.Value).GreaterThanOrEqualTo(0));
        When(x => x.TaxAmt.HasValue, () => RuleFor(x => x.TaxAmt!.Value).GreaterThanOrEqualTo(0));
        When(x => x.Freight.HasValue, () => RuleFor(x => x.Freight!.Value).GreaterThanOrEqualTo(0));
        When(x => x.PurchaseOrderNumber is not null, () => RuleFor(x => x.PurchaseOrderNumber!).MaximumLength(25));
        When(x => x.AccountNumber is not null, () => RuleFor(x => x.AccountNumber!).MaximumLength(15));
        When(x => x.CreditCardApprovalCode is not null, () => RuleFor(x => x.CreditCardApprovalCode!).MaximumLength(15));
        When(x => x.Comment is not null, () => RuleFor(x => x.Comment!).MaximumLength(128));
    }
}
