using FluentValidation;
using KromicStore.Domain.CustomerPortal.Entities;

namespace KromicStore.Application.CustomerPortal.Commands.UpdateNotificationPreferences;

public sealed class UpdateNotificationPreferencesValidator : AbstractValidator<UpdateNotificationPreferencesCommand>
{
    public UpdateNotificationPreferencesValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("Customer ID is required");
        
        RuleFor(x => x.NotificationType)
            .IsInEnum().WithMessage("Invalid notification type");
        
        RuleFor(x => x)
            .Must(x => x.EmailEnabled || x.SMSEnabled || x.PushEnabled || x.InAppEnabled)
            .WithMessage("At least one notification channel must be enabled");
        
        RuleFor(x => x.Frequency)
            .IsInEnum().WithMessage("Invalid notification frequency");
    }
}
