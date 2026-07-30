using FluentValidation;

namespace KromicStore.Application.Features.Catalog.Commands.DeleteVariant;

public sealed class DeleteVariantCommandValidator : AbstractValidator<DeleteVariantCommand>
{
    public DeleteVariantCommandValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("ProductId is required");

        RuleFor(x => x.VariantId)
            .NotEmpty().WithMessage("VariantId is required");
    }
}
