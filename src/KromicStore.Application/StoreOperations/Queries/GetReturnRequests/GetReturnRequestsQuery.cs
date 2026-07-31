using MediatR;

namespace KromicStore.Application.StoreOperations.Queries.GetReturnRequests;

/// <summary>
/// Query to retrieve return requests with filtering and pagination.
/// </summary>
public sealed class GetReturnRequestsQuery : IRequest<GetReturnRequestsResponse>
{
    public string? Status { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public sealed class ReturnRequestDto
{
    public Guid ReturnRequestId { get; set; }
    public Guid OrderId { get; set; }
    public Guid CustomerId { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime RequestedOnUtc { get; set; }
    public string Reason { get; set; } = string.Empty;
    public int ItemCount { get; set; }
    public decimal ReturnAmount { get; set; }
    public DateTime? ApprovedOnUtc { get; set; }
    public DateTime? CompletedOnUtc { get; set; }
}

public sealed class GetReturnRequestsResponse
{
    public List<ReturnRequestDto> ReturnRequests { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}
