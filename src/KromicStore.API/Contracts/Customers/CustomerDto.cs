namespace KromicStore.API.Contracts.Customers;

/// <summary>
/// DTO representing a customer.
/// </summary>
public class CustomerDto
{
    /// <summary>
    /// Unique customer identifier.
    /// </summary>
    public Guid CustomerId { get; set; }

    /// <summary>
    /// Customer's full name.
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Customer's email address.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Customer's phone number (optional).
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Total number of orders placed.
    /// </summary>
    public int TotalOrders { get; set; }

    /// <summary>
    /// Total amount spent.
    /// </summary>
    public decimal TotalSpent { get; set; }

    /// <summary>
    /// Average order value.
    /// </summary>
    public decimal AverageOrderValue { get; set; }

    /// <summary>
    /// When customer account was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// When customer account was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Customer preferences.
    /// </summary>
    public CustomerPreferencesDto? Preferences { get; set; }
}
