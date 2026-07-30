using FluentValidation;

namespace KromicStore.Application.Features.Shopping.Commands.ClearCart;

/// <summary>
/// Validator for ClearCart command.
/// Validates cart ID.
/// </summary>
public sealed class ClearCartCommandValidator : AbstractValidator<ClearCartCommand>
{
    public ClearCartCommandValidator()
    {
        RuleFor(x => x.CartId)
            .NotEmpty().WithMessage("CartId is required");
    }
}
