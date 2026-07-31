using FluentValidation;

namespace KromicStore.Application.Catalog.Commands.AdjustInventory;

/// <summary>
/// Validator for AdjustInventoryCommand.
/// </summary>
public sealed class AdjustInventoryValidator : AbstractValidator<AdjustInventoryCommand>
{
    public AdjustInventoryValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEqual(Guid.Empty).WithMessage("Product ID is required");

        RuleFor(x => x.AdjustmentQuantity)
            .NotEqual(0).WithMessage("Adjustment quantity must not be zero");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Adjustment reason is required")
            .MaximumLength(100).WithMessage("Adjustment reason must not exceed 100 characters");

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("Adjustment notes must not exceed 500 characters");
    }
}
