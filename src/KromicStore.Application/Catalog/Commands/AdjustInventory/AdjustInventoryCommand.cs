using MediatR;

namespace KromicStore.Application.Catalog.Commands.AdjustInventory;

/// <summary>
/// Command to adjust product inventory quantity.
/// </summary>
public sealed class AdjustInventoryCommand : IRequest<AdjustInventoryResponse>
{
    public Guid ProductId { get; set; }
    public Guid? VariantId { get; set; }
    public int AdjustmentQuantity { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? Notes { get; set; }
}

public sealed class AdjustInventoryResponse
{
    public Guid ProductId { get; set; }
    public Guid? VariantId { get; set; }
    public int CurrentQuantity { get; set; }
    public int AdjustmentQuantity { get; set; }
    public DateTime AdjustedOnUtc { get; set; }
}
