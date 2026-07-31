using MediatR;

namespace KromicStore.Application.CustomerPortal.Queries.GetOrders;

/// <summary>
/// Query to retrieve customer order history with pagination and filtering.
/// </summary>
public sealed class GetOrdersQuery : IRequest<GetOrdersResponse>
{
    public Guid CustomerId { get; set; }
    public string? OrderStatus { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public sealed class OrderSummaryDto
{
    public Guid OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public DateTime OrderDateUtc { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public int ItemCount { get; set; }
    public string? TrackingNumber { get; set; }
}

public sealed class GetOrdersResponse
{
    public List<OrderSummaryDto> Orders { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}
