using MediatR;

namespace KromicStore.Application.Features.Orders.Queries.GetOrders;

public record OrderSummaryDto(
    Guid Id,
    string OrderNumber,
    DateTime OrderDateUtc,
    decimal Total,
    string Status,
    int ItemCount);

/// <summary>
/// Query to retrieve paginated list of orders for a tenant or customer.
/// Supports filtering by status and date range.
/// </summary>
public sealed class GetOrdersQuery : IRequest<GetOrdersResponse>
{
    /// <summary>
    /// The tenant ID for tenant-portal requests, or null for customer-portal.
    /// </summary>
    public Guid? TenantId { get; set; }

    /// <summary>
    /// The customer ID for customer-portal requests, or null for tenant-portal.
    /// </summary>
    public Guid? CustomerId { get; set; }

    /// <summary>
    /// Optional status filter (e.g., "Pending", "Confirmed", "Shipped", "Delivered").
    /// </summary>
    public string? Status { get; set; }

    /// <summary>
    /// Skip count for pagination (default: 0).
    /// </summary>
    public int Skip { get; set; }

    /// <summary>
    /// Take count for pagination (default: 20, max: 100).
    /// </summary>
    public int Take { get; set; } = 20;
}

public sealed class GetOrdersResponse
{
    public List<OrderSummaryDto> Orders { get; set; } = [];
    public int TotalCount { get; set; }
    public int Skip { get; set; }
    public int Take { get; set; }
}
