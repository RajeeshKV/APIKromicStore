using MediatR;
using KromicStore.Domain.StoreOperations.Entities;

namespace KromicStore.Application.StoreOperations.Commands.ApproveAdjustment;

/// <summary>
/// Command to approve a pending inventory adjustment.
/// </summary>
public sealed class ApproveAdjustmentCommand : IRequest<ApproveAdjustmentResponse>
{
    public Guid AdjustmentId { get; set; }
}

public sealed class ApproveAdjustmentResponse
{
    public Guid AdjustmentId { get; set; }
    public AdjustmentStatus Status { get; set; }
    public DateTime ApprovedOnUtc { get; set; }
}
