using FluentValidation;

namespace KromicStore.Application.Catalog.Commands.AddProductVariant;

/// <summary>
/// Validator for AddProductVariantCommand.
/// </summary>
public sealed class AddProductVariantValidator : AbstractValidator<AddProductVariantCommand>
{
    public AddProductVariantValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEqual(Guid.Empty).WithMessage("Product ID is required");

        RuleFor(x => x.Sku)
            .NotEmpty().WithMessage("Variant SKU is required")
            .MaximumLength(50).WithMessage("Variant SKU must not exceed 50 characters");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Variant name is required")
            .MaximumLength(100).WithMessage("Variant name must not exceed 100 characters");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Variant price must be greater than 0");

        RuleFor(x => x.Quantity)
            .GreaterThanOrEqualTo(0).WithMessage("Variant quantity must be non-negative");
    }
}
