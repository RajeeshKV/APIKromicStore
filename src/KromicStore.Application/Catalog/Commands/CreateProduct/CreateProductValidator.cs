using FluentValidation;

namespace KromicStore.Application.Catalog.Commands.CreateProduct;

/// <summary>
/// Validator for CreateProductCommand.
/// </summary>
public sealed class CreateProductValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Product name is required")
            .MaximumLength(200).WithMessage("Product name must not exceed 200 characters");

        RuleFor(x => x.Sku)
            .NotEmpty().WithMessage("Product SKU is required")
            .MaximumLength(50).WithMessage("Product SKU must not exceed 50 characters")
            .Matches(@"^[A-Z0-9\-]*$").WithMessage("SKU must contain only uppercase letters, numbers, and hyphens");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Product description must not exceed 2000 characters");

        RuleFor(x => x.Slug)
            .MaximumLength(200).WithMessage("Product slug must not exceed 200 characters")
            .Matches(@"^[a-z0-9\-]*$").WithMessage("Slug must contain only lowercase letters, numbers, and hyphens");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be greater than 0");

        RuleFor(x => x.CompareAtPrice)
            .GreaterThan(x => x.Price)
            .When(x => x.CompareAtPrice.HasValue)
            .WithMessage("Compare at price must be greater than the regular price");

        RuleFor(x => x.Quantity)
            .GreaterThanOrEqualTo(0).WithMessage("Quantity must be non-negative");

        RuleFor(x => x.MetaTitle)
            .MaximumLength(60).WithMessage("Meta title must not exceed 60 characters");

        RuleFor(x => x.MetaDescription)
            .MaximumLength(160).WithMessage("Meta description must not exceed 160 characters");

        RuleFor(x => x.MetaKeywords)
            .MaximumLength(200).WithMessage("Meta keywords must not exceed 200 characters");

        RuleFor(x => x.Tags)
            .Must(tags => tags.All(t => t.Length <= 50))
            .WithMessage("Each tag must not exceed 50 characters");
    }
}
