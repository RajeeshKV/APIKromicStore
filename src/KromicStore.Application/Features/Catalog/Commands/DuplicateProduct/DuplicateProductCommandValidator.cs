using FluentValidation;
using KromicStore.Application.Features.Catalog.Abstractions;

namespace KromicStore.Application.Features.Catalog.Commands.DuplicateProduct;

public sealed class DuplicateProductCommandValidator : AbstractValidator<DuplicateProductCommand>
{
    private readonly IProductRepository _productRepository;

    public DuplicateProductCommandValidator(IProductRepository productRepository)
    {
        _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));

        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("ProductId is required");

        RuleFor(x => x.NewSku)
            .NotEmpty().WithMessage("NewSku is required")
            .MaximumLength(50).WithMessage("NewSku cannot exceed 50 characters")
            .Matches(@"^[A-Z0-9\-\.]+$").WithMessage("NewSku must contain only uppercase letters, numbers, hyphens, and periods");

        RuleFor(x => x.NewName)
            .NotEmpty().WithMessage("NewName is required")
            .MaximumLength(200).WithMessage("NewName cannot exceed 200 characters");

        RuleFor(x => x.NewSlug)
            .MaximumLength(200).WithMessage("NewSlug cannot exceed 200 characters")
            .Matches(@"^[a-z0-9]([a-z0-9\-]*[a-z0-9])?$").WithMessage("NewSlug must contain only lowercase letters, numbers, and hyphens")
            .When(x => !string.IsNullOrEmpty(x.NewSlug));
    }
}
