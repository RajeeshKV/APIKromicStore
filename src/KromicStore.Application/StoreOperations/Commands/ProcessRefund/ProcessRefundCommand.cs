using MediatR;

namespace KromicStore.Application.StoreOperations.Commands.ProcessRefund;

/// <summary>
/// Command to process a refund for a return request.
/// </summary>
public sealed class ProcessRefundCommand : IRequest<ProcessRefundResponse>
{
    public Guid ReturnRequestId { get; set; }
    public decimal RefundAmount { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public sealed class ProcessRefundResponse
{
    public Guid ReturnRequestId { get; set; }
    public decimal RefundAmount { get; set; }
    public DateTime ProcessedOnUtc { get; set; }
}
