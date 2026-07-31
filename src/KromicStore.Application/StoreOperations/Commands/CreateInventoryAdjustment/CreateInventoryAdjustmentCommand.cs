using MediatR;
using KromicStore.Domain.StoreOperations.Entities;

namespace KromicStore.Application.StoreOperations.Commands.CreateInventoryAdjustment;

/// <summary>
/// Command to create a new inventory adjustment request.
/// </summary>
public sealed class CreateInventoryAdjustmentCommand : IRequest<CreateInventoryAdjustmentResponse>
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public AdjustmentReason Reason { get; set; }
    public string ReasonNotes { get; set; } = string.Empty;
}

public sealed class CreateInventoryAdjustmentResponse
{
    public Guid AdjustmentId { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public AdjustmentReason Reason { get; set; }
    public DateTime CreatedOnUtc { get; set; }
}
