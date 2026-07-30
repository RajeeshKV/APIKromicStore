using FluentValidation;

namespace KromicStore.Application.Features.Catalog.Commands.UpdateVariant;

public sealed class UpdateVariantCommandValidator : AbstractValidator<UpdateVariantCommand>
{
    public UpdateVariantCommandValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("ProductId is required");

        RuleFor(x => x.VariantId)
            .NotEmpty().WithMessage("VariantId is required");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(200).WithMessage("Name cannot exceed 200 characters")
            .When(x => x.Name is not null);

        RuleFor(x => x.PriceAdjustment)
            .GreaterThanOrEqualTo(0).WithMessage("PriceAdjustment cannot be negative")
            .When(x => x.PriceAdjustment.HasValue);

        RuleFor(x => x.Attributes)
            .Must(x => x == null || x.Count <= 20).WithMessage("Cannot have more than 20 attributes")
            .When(x => x.Attributes != null);
    }
}
