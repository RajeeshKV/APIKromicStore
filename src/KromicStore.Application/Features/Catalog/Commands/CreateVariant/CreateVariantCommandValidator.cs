using FluentValidation;
using KromicStore.Application.Features.Catalog.Abstractions;

namespace KromicStore.Application.Features.Catalog.Commands.CreateVariant;

public sealed class CreateVariantCommandValidator : AbstractValidator<CreateVariantCommand>
{
    private readonly IProductRepository _productRepository;

    public CreateVariantCommandValidator(IProductRepository productRepository)
    {
        _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));

        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("ProductId is required");

        RuleFor(x => x.SkuSuffix)
            .NotEmpty().WithMessage("SkuSuffix is required")
            .MaximumLength(50).WithMessage("SkuSuffix cannot exceed 50 characters")
            .Matches(@"^[A-Z0-9\-\.]+$").WithMessage("SkuSuffix must contain only uppercase letters, numbers, hyphens, and periods");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(200).WithMessage("Name cannot exceed 200 characters");

        RuleFor(x => x.PriceAdjustment)
            .GreaterThanOrEqualTo(0).WithMessage("PriceAdjustment cannot be negative");

        RuleFor(x => x.StockQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("StockQuantity cannot be negative");

        RuleFor(x => x.Attributes)
            .Must(x => x == null || x.Count <= 20).WithMessage("Cannot have more than 20 attributes")
            .When(x => x.Attributes != null);
    }
}
