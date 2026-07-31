using MediatR;

namespace KromicStore.Application.StoreOperations.Queries.GetPendingFulfillments;

/// <summary>
/// Query to retrieve pending fulfillments requiring action.
/// </summary>
public sealed class GetPendingFulfillmentsQuery : IRequest<GetPendingFulfillmentsResponse>
{
    public string? Status { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public sealed class PendingFulfillmentDto
{
    public Guid FulfillmentId { get; set; }
    public Guid OrderId { get; set; }
    public string Status { get; set; } = string.Empty;
    public int ItemCount { get; set; }
    public string ShippingAddress { get; set; } = string.Empty;
    public decimal ShippingCost { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? ProcessedAtUtc { get; set; }
    public string? TrackingNumber { get; set; }
}

public sealed class GetPendingFulfillmentsResponse
{
    public List<PendingFulfillmentDto> Fulfillments { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}
