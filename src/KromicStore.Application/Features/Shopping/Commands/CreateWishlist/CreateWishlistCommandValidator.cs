using FluentValidation;

namespace KromicStore.Application.Features.Shopping.Commands.CreateWishlist;

/// <summary>
/// Validator for CreateWishlist command.
/// Validates customer ID.
/// </summary>
public sealed class CreateWishlistCommandValidator : AbstractValidator<CreateWishlistCommand>
{
    public CreateWishlistCommandValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("CustomerId is required");
    }
}
