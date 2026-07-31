using FluentValidation;
using KromicStore.Domain.Promotions.Entities;

namespace KromicStore.Application.Features.Promotions.Commands.CreateDiscount;

public sealed class CreateDiscountCommandValidator : AbstractValidator<CreateDiscountCommand>
{
    public CreateDiscountCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Discount name is required")
            .MaximumLength(200)
            .WithMessage("Discount name cannot exceed 200 characters");
        
        RuleFor(x => x.Description)
            .MaximumLength(1000)
            .WithMessage("Description cannot exceed 1000 characters");
        
        RuleFor(x => x.Type)
            .IsInEnum()
            .WithMessage("Invalid discount type");
        
        RuleFor(x => x.FixedAmount)
            .GreaterThan(0)
            .WithMessage("Fixed amount must be greater than 0")
            .When(x => x.Type == DiscountType.FixedAmount);
        
        RuleFor(x => x.PercentageAmount)
            .GreaterThan(0)
            .WithMessage("Percentage must be greater than 0")
            .LessThanOrEqualTo(1)
            .WithMessage("Percentage cannot exceed 100%")
            .When(x => x.Type == DiscountType.PercentageAmount);
        
        RuleFor(x => x.MaxDiscountAmount)
            .GreaterThan(0)
            .WithMessage("Max discount amount must be greater than 0")
            .When(x => x.Type == DiscountType.PercentageAmount && x.MaxDiscountAmount.HasValue);
        
        RuleFor(x => x.ValidFromUtc)
            .NotEmpty()
            .WithMessage("Valid from date is required");
        
        RuleFor(x => x.ValidToUtc)
            .GreaterThan(x => x.ValidFromUtc)
            .WithMessage("Valid to date must be after valid from date");
        
        RuleFor(x => x.MaxUsageCount)
            .GreaterThan(0)
            .WithMessage("Max usage count must be greater than 0 or -1 for unlimited")
            .When(x => x.MaxUsageCount != -1);
    }
}
