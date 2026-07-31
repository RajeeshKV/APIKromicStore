using FluentValidation;

namespace KromicStore.Application.StoreOperations.Commands.ApproveAdjustment;

public sealed class ApproveAdjustmentValidator : AbstractValidator<ApproveAdjustmentCommand>
{
    public ApproveAdjustmentValidator()
    {
        RuleFor(x => x.AdjustmentId)
            .NotEmpty().WithMessage("Adjustment ID is required");
    }
}
