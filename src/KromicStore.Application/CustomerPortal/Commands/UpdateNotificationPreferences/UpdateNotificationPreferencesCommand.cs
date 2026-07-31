using MediatR;
using KromicStore.Domain.CustomerPortal.Entities;

namespace KromicStore.Application.CustomerPortal.Commands.UpdateNotificationPreferences;

/// <summary>
/// Command to update customer notification preferences.
/// </summary>
public sealed class UpdateNotificationPreferencesCommand : IRequest<UpdateNotificationPreferencesResponse>
{
    public Guid CustomerId { get; set; }
    public NotificationType NotificationType { get; set; }
    public bool EmailEnabled { get; set; }
    public bool SMSEnabled { get; set; }
    public bool PushEnabled { get; set; }
    public bool InAppEnabled { get; set; }
    public NotificationFrequency Frequency { get; set; }
}

public sealed class UpdateNotificationPreferencesResponse
{
    public Guid PreferenceId { get; set; }
    public NotificationType NotificationType { get; set; }
    public bool EmailEnabled { get; set; }
    public bool SMSEnabled { get; set; }
    public bool PushEnabled { get; set; }
    public bool InAppEnabled { get; set; }
    public NotificationFrequency Frequency { get; set; }
    public DateTime ModifiedOnUtc { get; set; }
}
