using MediatR;

namespace KromicStore.Application.Features.Orders.Queries.GetOrderTimeline;

/// <summary>
/// Query to retrieve order timeline events showing order status changes and activities.
/// </summary>
public sealed class GetOrderTimelineQuery : IRequest<GetOrderTimelineResponse>
{
    public Guid OrderId { get; set; }

    /// <summary>
    /// Optional: Customer ID for customer-portal access validation.
    /// </summary>
    public Guid? CustomerId { get; set; }

    /// <summary>
    /// Optional: Tenant ID for tenant-portal access validation.
    /// </summary>
    public Guid? TenantId { get; set; }
}

public sealed class OrderTimelineEventDto
{
    public Guid EventId { get; set; }
    public string EventTitle { get; set; } = string.Empty;
    public string EventDescription { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public DateTime OccurredAtUtc { get; set; }
    public string? Notes { get; set; }
}

public sealed class GetOrderTimelineResponse
{
    public Guid OrderId { get; set; }
    public List<OrderTimelineEventDto> Events { get; set; } = [];
}
