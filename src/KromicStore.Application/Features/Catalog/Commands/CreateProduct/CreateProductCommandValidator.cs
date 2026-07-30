using FluentValidation;
using KromicStore.Application.Features.Catalog.Abstractions;

namespace KromicStore.Application.Features.Catalog.Commands.CreateProduct;

public sealed class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    private readonly IProductRepository _productRepository;

    public CreateProductCommandValidator(IProductRepository productRepository)
    {
        _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));

        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("CategoryId is required");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(200).WithMessage("Name cannot exceed 200 characters");

        RuleFor(x => x.Sku)
            .NotEmpty().WithMessage("SKU is required")
            .MaximumLength(50).WithMessage("SKU cannot exceed 50 characters")
            .Matches(@"^[A-Z0-9\-\.]+$").WithMessage("SKU must contain only uppercase letters, numbers, hyphens, and periods");

        RuleFor(x => x.ShortDescription)
            .MaximumLength(255).WithMessage("ShortDescription cannot exceed 255 characters");

        RuleFor(x => x.Description)
            .MaximumLength(5000).WithMessage("Description cannot exceed 5000 characters");

        RuleFor(x => x.CustomSlug)
            .MaximumLength(200).WithMessage("CustomSlug cannot exceed 200 characters")
            .Matches(@"^[a-z0-9]([a-z0-9\-]*[a-z0-9])?$").WithMessage("Slug must contain only lowercase letters, numbers, and hyphens")
            .When(x => !string.IsNullOrEmpty(x.CustomSlug));

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).WithMessage("Price cannot be negative");

        RuleFor(x => x.CompareAtPrice)
            .GreaterThanOrEqualTo(0).WithMessage("CompareAtPrice cannot be negative")
            .GreaterThan(x => x.Price).WithMessage("CompareAtPrice must be greater than Price")
            .When(x => x.CompareAtPrice.HasValue);

        RuleFor(x => x.CostPrice)
            .GreaterThanOrEqualTo(0).WithMessage("CostPrice cannot be negative")
            .When(x => x.CostPrice.HasValue);

        RuleFor(x => x.Weight)
            .GreaterThan(0).WithMessage("Weight must be greater than 0")
            .When(x => x.Weight.HasValue);

        RuleFor(x => x.Length)
            .GreaterThan(0).WithMessage("Length must be greater than 0")
            .When(x => x.Length.HasValue);

        RuleFor(x => x.Width)
            .GreaterThan(0).WithMessage("Width must be greater than 0")
            .When(x => x.Width.HasValue);

        RuleFor(x => x.Height)
            .GreaterThan(0).WithMessage("Height must be greater than 0")
            .When(x => x.Height.HasValue);

        RuleFor(x => x.Attributes)
            .Must(x => x == null || x.Count <= 50).WithMessage("Cannot have more than 50 attributes")
            .When(x => x.Attributes != null);

        RuleFor(x => x.Tags)
            .Must(x => x == null || x.Count <= 20).WithMessage("Cannot have more than 20 tags")
            .When(x => x.Tags != null);
    }
}
