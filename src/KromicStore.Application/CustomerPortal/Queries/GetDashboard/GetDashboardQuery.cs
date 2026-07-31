using MediatR;

namespace KromicStore.Application.CustomerPortal.Queries.GetDashboard;

/// <summary>
/// Query to retrieve customer dashboard with summary information.
/// </summary>
public sealed class GetDashboardQuery : IRequest<GetDashboardResponse>
{
    public Guid CustomerId { get; set; }
}

public sealed class DashboardOrderSummary
{
    public int TotalOrders { get; set; }
    public int PendingOrders { get; set; }
    public int ShippedOrders { get; set; }
    public int DeliveredOrders { get; set; }
    public decimal TotalSpent { get; set; }
}

public sealed class GetDashboardResponse
{
    public Guid CustomerId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public int SavedAddressesCount { get; set; }
    public int WishlistCount { get; set; }
    public int UnreadNotificationsCount { get; set; }
    public DashboardOrderSummary OrderSummary { get; set; } = new();
    public DateTime? LastLoginUtc { get; set; }
    public bool NewsletterOptIn { get; set; }
}
