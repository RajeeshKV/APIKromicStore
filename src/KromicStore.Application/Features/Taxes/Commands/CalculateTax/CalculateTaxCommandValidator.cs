using FluentValidation;

namespace KromicStore.Application.Features.Taxes.Commands.CalculateTax;

public sealed class CalculateTaxCommandValidator : AbstractValidator<CalculateTaxCommand>
{
    public CalculateTaxCommandValidator()
    {
        RuleFor(x => x.TaxRegionId)
            .NotEmpty()
            .WithMessage("Tax region is required");
        
        RuleFor(x => x.ProductCategory)
            .NotEmpty()
            .WithMessage("Product category is required")
            .MaximumLength(100)
            .WithMessage("Product category cannot exceed 100 characters");
        
        RuleFor(x => x.OrderAmount)
            .GreaterThan(0)
            .WithMessage("Order amount must be greater than 0");
    }
}
