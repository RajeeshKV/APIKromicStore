using KromicStore.Domain.Common;

namespace KromicStore.Domain.CustomerPortal.Entities;

/// <summary>
/// Notification types that customers can control preferences for.
/// </summary>
public enum NotificationType
{
    OrderUpdate = 0,
    ShipmentTracking = 1,
    NewProducts = 2,
    Promotions = 3,
    AccountUpdates = 4,
    Reviews = 5,
    Surveys = 6
}

/// <summary>
/// Notification channels available.
/// </summary>
public enum NotificationChannel
{
    Email = 0,
    SMS = 1,
    Push = 2,
    InApp = 3
}

/// <summary>
/// Notification frequency preferences.
/// </summary>
public enum NotificationFrequency
{
    RealTime = 0,
    Daily = 1,
    Weekly = 2,
    Monthly = 3,
    Never = 4
}

/// <summary>
/// CustomerNotificationPreference represents a customer's preferences for notifications.
/// Allows granular control over which notifications to receive via which channels.
/// </summary>
public sealed class CustomerNotificationPreference : TenantEntity, IAuditable
{
    public Guid CustomerId { get; private set; }
    public NotificationType NotificationType { get; private set; }
    
    // Channel preferences
    public bool EmailEnabled { get; private set; }
    public bool SMSEnabled { get; private set; }
    public bool PushEnabled { get; private set; }
    public bool InAppEnabled { get; private set; }
    
    // Frequency
    public NotificationFrequency Frequency { get; private set; }
    
    // Auditing is inherited from AuditableEntity
    
    private CustomerNotificationPreference()
    {
    }
    
    private CustomerNotificationPreference(Guid id, Guid tenantId) : base(id, tenantId)
    {
    }
    
    /// <summary>
    /// Create a new notification preference with defaults.
    /// </summary>
    public static CustomerNotificationPreference Create(
        Guid tenantId,
        Guid customerId,
        NotificationType notificationType)
    {
        var preference = new CustomerNotificationPreference(Guid.NewGuid(), tenantId)
        {
            CustomerId = customerId,
            NotificationType = notificationType,
            EmailEnabled = true,
            SMSEnabled = false,
            PushEnabled = true,
            InAppEnabled = true,
            Frequency = NotificationFrequency.RealTime
        };
        
        return preference;
    }
    
    /// <summary>
    /// Check if customer wants to receive this notification via any channel.
    /// </summary>
    public bool IsEnabled() => EmailEnabled || SMSEnabled || PushEnabled || InAppEnabled;
    
    /// <summary>
    /// Check if customer wants to receive via specific channel.
    /// </summary>
    public bool IsChannelEnabled(NotificationChannel channel) => channel switch
    {
        NotificationChannel.Email => EmailEnabled,
        NotificationChannel.SMS => SMSEnabled,
        NotificationChannel.Push => PushEnabled,
        NotificationChannel.InApp => InAppEnabled,
        _ => false
    };
    
    /// <summary>
    /// Update notification preference channels.
    /// </summary>
    public void UpdateChannels(bool email, bool sms, bool push, bool inApp)
    {
        if (!email && !sms && !push && !inApp)
            throw new InvalidOperationException("At least one notification channel must be enabled");
        
        EmailEnabled = email;
        SMSEnabled = sms;
        PushEnabled = push;
        InAppEnabled = inApp;
    }
    
    /// <summary>
    /// Enable specific channel.
    /// </summary>
    public void EnableChannel(NotificationChannel channel)
    {
        switch (channel)
        {
            case NotificationChannel.Email:
                EmailEnabled = true;
                break;
            case NotificationChannel.SMS:
                SMSEnabled = true;
                break;
            case NotificationChannel.Push:
                PushEnabled = true;
                break;
            case NotificationChannel.InApp:
                InAppEnabled = true;
                break;
        }
    }
    
    /// <summary>
    /// Disable specific channel (fails if last enabled channel).
    /// </summary>
    public void DisableChannel(NotificationChannel channel)
    {
        // Check if this is the last enabled channel
        var enabledCount = (EmailEnabled ? 1 : 0) +
                          (SMSEnabled ? 1 : 0) +
                          (PushEnabled ? 1 : 0) +
                          (InAppEnabled ? 1 : 0);
        
        if (enabledCount <= 1)
            throw new InvalidOperationException("Cannot disable the last notification channel");
        
        switch (channel)
        {
            case NotificationChannel.Email:
                EmailEnabled = false;
                break;
            case NotificationChannel.SMS:
                SMSEnabled = false;
                break;
            case NotificationChannel.Push:
                PushEnabled = false;
                break;
            case NotificationChannel.InApp:
                InAppEnabled = false;
                break;
        }
    }
    
    /// <summary>
    /// Update notification frequency.
    /// </summary>
    public void SetFrequency(NotificationFrequency frequency)
    {
        Frequency = frequency;
    }
    
    /// <summary>
    /// Disable all notifications for this type.
    /// </summary>
    public void DisableAll()
    {
        EmailEnabled = false;
        SMSEnabled = false;
        PushEnabled = false;
        InAppEnabled = false;
    }
    
    /// <summary>
    /// Enable default channels (Email and InApp).
    /// </summary>
    public void EnableDefaults()
    {
        EmailEnabled = true;
        SMSEnabled = false;
        PushEnabled = false;
        InAppEnabled = true;
        Frequency = NotificationFrequency.RealTime;
    }
}
