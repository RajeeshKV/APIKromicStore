namespace KromicStore.API.Contracts.Customers;

/// <summary>
/// Request to update customer preferences.
/// </summary>
public class UpdateCustomerPreferencesRequest
{
    /// <summary>
    /// Whether customer opted in to marketing emails.
    /// </summary>
    public bool NewsletterOptIn { get; set; }

    /// <summary>
    /// Whether customer opted in to order notifications.
    /// </summary>
    public bool OrderNotifications { get; set; }

    /// <summary>
    /// Preferred communication language.
    /// </summary>
    public string PreferredLanguage { get; set; } = "en";

    /// <summary>
    /// Preferred contact method (email, sms, phone).
    /// </summary>
    public string PreferredContactMethod { get; set; } = "email";
}
