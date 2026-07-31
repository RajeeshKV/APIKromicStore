using FluentValidation;

namespace KromicStore.Application.StoreOperations.Commands.RequestReturn;

public sealed class RequestReturnValidator : AbstractValidator<RequestReturnCommand>
{
    public RequestReturnValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("Order ID is required");
        
        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("Customer ID is required");
        
        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Reason is required")
            .MaximumLength(200).WithMessage("Reason cannot exceed 200 characters");
        
        RuleFor(x => x.CustomerNotes)
            .MaximumLength(500).WithMessage("Customer notes cannot exceed 500 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.CustomerNotes));
        
        RuleFor(x => x.ItemCount)
            .GreaterThan(0).WithMessage("Item count must be greater than zero");
        
        RuleFor(x => x.ReturnAmount)
            .GreaterThan(0).WithMessage("Return amount must be greater than zero");
    }
}
