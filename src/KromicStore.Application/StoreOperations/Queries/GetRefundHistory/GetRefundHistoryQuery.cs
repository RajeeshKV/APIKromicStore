using MediatR;

namespace KromicStore.Application.StoreOperations.Queries.GetRefundHistory;

/// <summary>
/// Query to retrieve refund processing history.
/// </summary>
public sealed class GetRefundHistoryQuery : IRequest<GetRefundHistoryResponse>
{
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public sealed class RefundDto
{
    public Guid RefundId { get; set; }
    public Guid ReturnRequestId { get; set; }
    public Guid OrderId { get; set; }
    public Guid CustomerId { get; set; }
    public decimal RefundAmount { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTime ProcessedOnUtc { get; set; }
}

public sealed class GetRefundHistoryResponse
{
    public List<RefundDto> Refunds { get; set; } = new();
    public decimal TotalRefunded { get; set; }
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}
