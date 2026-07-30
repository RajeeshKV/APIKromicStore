using FluentValidation;

namespace KromicStore.Application.Features.Catalog.Commands.CreateProductImage;

public sealed class CreateProductImageCommandValidator : AbstractValidator<CreateProductImageCommand>
{
    public CreateProductImageCommandValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("ProductId is required");

        RuleFor(x => x.ImageUrl)
            .NotEmpty().WithMessage("ImageUrl is required")
            .MaximumLength(500).WithMessage("ImageUrl cannot exceed 500 characters")
            .Must(x => Uri.TryCreate(x, UriKind.Absolute, out _)).WithMessage("ImageUrl must be a valid URL");

        RuleFor(x => x.AltText)
            .MaximumLength(255).WithMessage("AltText cannot exceed 255 characters")
            .When(x => !string.IsNullOrEmpty(x.AltText));
    }
}
