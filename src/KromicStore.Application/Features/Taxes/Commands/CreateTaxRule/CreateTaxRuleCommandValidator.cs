using FluentValidation;

namespace KromicStore.Application.Features.Taxes.Commands.CreateTaxRule;

public sealed class CreateTaxRuleCommandValidator : AbstractValidator<CreateTaxRuleCommand>
{
    public CreateTaxRuleCommandValidator()
    {
        RuleFor(x => x.TaxRegionId)
            .NotEmpty()
            .WithMessage("Tax region is required");
        
        RuleFor(x => x.ProductCategory)
            .NotEmpty()
            .WithMessage("Product category is required")
            .MaximumLength(200)
            .WithMessage("Product category cannot exceed 200 characters");
        
        RuleFor(x => x.TaxRate)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Tax rate cannot be negative")
            .LessThanOrEqualTo(1)
            .WithMessage("Tax rate cannot exceed 100%");
        
        RuleFor(x => x.Description)
            .MaximumLength(1000)
            .WithMessage("Description cannot exceed 1000 characters");
        
        RuleFor(x => x.EffectiveToUtc)
            .GreaterThan(x => x.EffectiveFromUtc)
            .WithMessage("Effective to date must be after effective from date")
            .When(x => x.EffectiveFromUtc.HasValue && x.EffectiveToUtc.HasValue);
    }
}
