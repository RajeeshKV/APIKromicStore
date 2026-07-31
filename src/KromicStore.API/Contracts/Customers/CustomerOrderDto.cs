namespace KromicStore.API.Contracts.Customers;

/// <summary>
/// DTO representing a customer's order summary.
/// </summary>
public class CustomerOrderDto
{
    /// <summary>
    /// Order identifier.
    /// </summary>
    public Guid OrderId { get; set; }

    /// <summary>
    /// Order number for display.
    /// </summary>
    public string OrderNumber { get; set; } = string.Empty;

    /// <summary>
    /// Order status (Pending, Confirmed, Dispatched, Delivered, Cancelled).
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Total order amount.
    /// </summary>
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// Number of items in order.
    /// </summary>
    public int ItemCount { get; set; }

    /// <summary>
    /// When order was placed.
    /// </summary>
    public DateTime CreatedAt { get; set; }
}
