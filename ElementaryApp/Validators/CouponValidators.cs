using ElementaryApp.Models;
using FluentValidation;

namespace ElementaryApp.Validators;

public sealed class CreateCouponValidator : AbstractValidator<CreateCouponRequest>
{
    public CreateCouponValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id is required.")
            .MaximumLength(64);

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.")
            .MaximumLength(256);

        RuleFor(x => x.Discount)
            .GreaterThan(0).WithMessage("Discount must be greater than 0.")
            .LessThanOrEqualTo(100).WithMessage("Discount cannot exceed 100.");

        RuleFor(x => x.ExpiryDate)
            .NotEqual(default(DateTime)).WithMessage("ExpiryDate is required.");
    }
}

public sealed class UpdateCouponValidator : AbstractValidator<UpdateCouponRequest>
{
    public UpdateCouponValidator()
    {
        RuleFor(x => x.Description!)
            .NotEmpty().MaximumLength(256)
            .When(x => x.Description is not null);

        RuleFor(x => x.Discount!.Value)
            .GreaterThan(0).WithMessage("Discount must be greater than 0.")
            .LessThanOrEqualTo(100).WithMessage("Discount cannot exceed 100.")
            .When(x => x.Discount.HasValue);
    }
}
