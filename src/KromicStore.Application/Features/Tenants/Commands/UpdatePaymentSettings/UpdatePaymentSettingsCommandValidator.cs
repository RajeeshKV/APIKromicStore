using FluentValidation;

namespace KromicStore.Application.Features.Tenants.Commands.UpdatePaymentSettings;

public sealed class UpdatePaymentSettingsCommandValidator : AbstractValidator<UpdatePaymentSettingsCommand>
{
    public UpdatePaymentSettingsCommandValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEqual(Guid.Empty).WithMessage("TenantId is required.");

        RuleFor(x => x.RazorpayKeyId)
            .NotEmpty().When(x => x.RazorpayEnabled).WithMessage("RazorpayKeyId is required when Razorpay is enabled.")
            .MinimumLength(5).When(x => x.RazorpayEnabled).WithMessage("RazorpayKeyId must be at least 5 characters.");

        RuleFor(x => x.RazorpayKeySecret)
            .NotEmpty().When(x => x.RazorpayEnabled).WithMessage("RazorpayKeySecret is required when Razorpay is enabled.")
            .MinimumLength(10).When(x => x.RazorpayEnabled).WithMessage("RazorpayKeySecret must be at least 10 characters.");
    }
}
