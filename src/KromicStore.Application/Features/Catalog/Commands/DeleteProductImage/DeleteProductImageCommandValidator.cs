using FluentValidation;

namespace KromicStore.Application.Features.Catalog.Commands.DeleteProductImage;

public sealed class DeleteProductImageCommandValidator : AbstractValidator<DeleteProductImageCommand>
{
    public DeleteProductImageCommandValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("ProductId is required");

        RuleFor(x => x.ImageId)
            .NotEmpty().WithMessage("ImageId is required");
    }
}
