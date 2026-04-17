using AWBlazorApp.Features.Inventory.Models;
using FluentValidation;

namespace AWBlazorApp.Features.Inventory.Validators;

// Validators for the inventory master-data create/update requests. Read-only DTOs
// (balances, transactions, reference tables) don't need validators.

public sealed class CreateInventoryLocationValidator : AbstractValidator<CreateInventoryLocationRequest>
{
    public CreateInventoryLocationValidator()
    {
        RuleFor(x => x.OrganizationId).GreaterThan(0);
        RuleFor(x => x.Code).NotEmpty().MaximumLength(64)
            .Matches("^[A-Z0-9][A-Z0-9_-]*$").WithMessage("Code must be uppercase letters/digits/underscore/hyphen.");
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
    }
}

public sealed class UpdateInventoryLocationValidator : AbstractValidator<UpdateInventoryLocationRequest>
{
    public UpdateInventoryLocationValidator()
    {
        When(x => x.Code is not null, () =>
            RuleFor(x => x.Code!).NotEmpty().MaximumLength(64)
                .Matches("^[A-Z0-9][A-Z0-9_-]*$").WithMessage("Code must be uppercase letters/digits/underscore/hyphen."));
        When(x => x.Name is not null, () =>
            RuleFor(x => x.Name!).NotEmpty().MaximumLength(200));
    }
}

public sealed class CreateInventoryItemValidator : AbstractValidator<CreateInventoryItemRequest>
{
    public CreateInventoryItemValidator()
    {
        RuleFor(x => x.ProductId).GreaterThan(0);
        RuleFor(x => x.MinQty).GreaterThanOrEqualTo(0);
        RuleFor(x => x.MaxQty).GreaterThanOrEqualTo(x => x.MinQty)
            .WithMessage("MaxQty must be greater than or equal to MinQty.");
        RuleFor(x => x.ReorderPoint).GreaterThanOrEqualTo(0);
        RuleFor(x => x.ReorderQty).GreaterThanOrEqualTo(0);
    }
}

public sealed class UpdateInventoryItemValidator : AbstractValidator<UpdateInventoryItemRequest>
{
    public UpdateInventoryItemValidator()
    {
        When(x => x.MinQty.HasValue, () => RuleFor(x => x.MinQty!.Value).GreaterThanOrEqualTo(0));
        When(x => x.MaxQty.HasValue, () => RuleFor(x => x.MaxQty!.Value).GreaterThanOrEqualTo(0));
        When(x => x.ReorderPoint.HasValue, () => RuleFor(x => x.ReorderPoint!.Value).GreaterThanOrEqualTo(0));
        When(x => x.ReorderQty.HasValue, () => RuleFor(x => x.ReorderQty!.Value).GreaterThanOrEqualTo(0));
    }
}

public sealed class CreateLotValidator : AbstractValidator<CreateLotRequest>
{
    public CreateLotValidator()
    {
        RuleFor(x => x.InventoryItemId).GreaterThan(0);
        RuleFor(x => x.LotCode).NotEmpty().MaximumLength(64);
    }
}

public sealed class UpdateLotValidator : AbstractValidator<UpdateLotRequest>
{
    public UpdateLotValidator()
    {
        When(x => x.LotCode is not null, () =>
            RuleFor(x => x.LotCode!).NotEmpty().MaximumLength(64));
    }
}

public sealed class CreateSerialUnitValidator : AbstractValidator<CreateSerialUnitRequest>
{
    public CreateSerialUnitValidator()
    {
        RuleFor(x => x.InventoryItemId).GreaterThan(0);
        RuleFor(x => x.SerialNumber).NotEmpty().MaximumLength(128);
    }
}

public sealed class UpdateSerialUnitValidator : AbstractValidator<UpdateSerialUnitRequest>
{
    public UpdateSerialUnitValidator()
    {
        When(x => x.SerialNumber is not null, () =>
            RuleFor(x => x.SerialNumber!).NotEmpty().MaximumLength(128));
    }
}

public sealed class CreateInventoryAdjustmentValidator : AbstractValidator<CreateInventoryAdjustmentRequest>
{
    public CreateInventoryAdjustmentValidator()
    {
        RuleFor(x => x.InventoryItemId).GreaterThan(0);
        RuleFor(x => x.LocationId).GreaterThan(0);
        RuleFor(x => x.QuantityDelta).NotEqual(0).WithMessage("QuantityDelta must be non-zero.");
        RuleFor(x => x.Reason).MaximumLength(500);
    }
}

public sealed class UpdateInventoryAdjustmentValidator : AbstractValidator<UpdateInventoryAdjustmentRequest>
{
    public UpdateInventoryAdjustmentValidator()
    {
        When(x => x.QuantityDelta.HasValue, () =>
            RuleFor(x => x.QuantityDelta!.Value).NotEqual(0));
        When(x => x.Reason is not null, () =>
            RuleFor(x => x.Reason!).MaximumLength(500));
    }
}

public sealed class PostInventoryTransactionValidator : AbstractValidator<PostInventoryTransactionRequest>
{
    public PostInventoryTransactionValidator()
    {
        RuleFor(x => x.TypeCode).NotEmpty().MaximumLength(32);
        RuleFor(x => x.InventoryItemId).GreaterThan(0);
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.UnitMeasureCode).NotEmpty().MaximumLength(3);
        RuleFor(x => x.Notes).MaximumLength(500);
    }
}
