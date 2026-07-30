using FluentValidation;

namespace KromicStore.Application.Features.Catalog.Commands.RestoreProduct;

public sealed class RestoreProductCommandValidator : AbstractValidator<RestoreProductCommand>
{
    public RestoreProductCommandValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("ProductId is required");
    }
}
