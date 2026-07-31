using FluentValidation;
using KromicStore.Domain.StoreOperations.Entities;

namespace KromicStore.Application.StoreOperations.Commands.CreateInventoryAdjustment;

public sealed class CreateInventoryAdjustmentValidator : AbstractValidator<CreateInventoryAdjustmentCommand>
{
    public CreateInventoryAdjustmentValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Product ID is required");
        
        RuleFor(x => x.Quantity)
            .NotEqual(0).WithMessage("Quantity cannot be zero");
        
        RuleFor(x => x.Reason)
            .IsInEnum().WithMessage("Invalid adjustment reason");
        
        RuleFor(x => x.ReasonNotes)
            .NotEmpty().WithMessage("Reason notes are required")
            .MaximumLength(500).WithMessage("Reason notes cannot exceed 500 characters");
    }
}
