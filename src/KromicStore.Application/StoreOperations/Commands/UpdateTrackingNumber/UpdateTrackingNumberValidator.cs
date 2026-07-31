using FluentValidation;

namespace KromicStore.Application.StoreOperations.Commands.UpdateTrackingNumber;

public sealed class UpdateTrackingNumberValidator : AbstractValidator<UpdateTrackingNumberCommand>
{
    public UpdateTrackingNumberValidator()
    {
        RuleFor(x => x.FulfillmentId)
            .NotEmpty().WithMessage("Fulfillment ID is required");
        
        RuleFor(x => x.TrackingNumber)
            .NotEmpty().WithMessage("Tracking number is required")
            .MaximumLength(100).WithMessage("Tracking number cannot exceed 100 characters");
        
        RuleFor(x => x.CarrierCode)
            .NotEmpty().WithMessage("Carrier code is required")
            .MaximumLength(20).WithMessage("Carrier code cannot exceed 20 characters");
    }
}
