using FluentValidation;

namespace KromicStore.Application.StoreOperations.Commands.ProcessRefund;

public sealed class ProcessRefundValidator : AbstractValidator<ProcessRefundCommand>
{
    public ProcessRefundValidator()
    {
        RuleFor(x => x.ReturnRequestId)
            .NotEmpty().WithMessage("Return request ID is required");
        
        RuleFor(x => x.RefundAmount)
            .GreaterThan(0).WithMessage("Refund amount must be greater than zero");
        
        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Reason is required")
            .MaximumLength(200).WithMessage("Reason cannot exceed 200 characters");
    }
}
