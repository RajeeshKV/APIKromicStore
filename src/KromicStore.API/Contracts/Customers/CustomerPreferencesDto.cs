namespace KromicStore.API.Contracts.Customers;

/// <summary>
/// DTO representing customer preferences.
/// </summary>
public class CustomerPreferencesDto
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
    /// Preferred contact method.
    /// </summary>
    public string PreferredContactMethod { get; set; } = "email";
}
