using MediatR;

namespace KromicStore.Application.Features.Catalog.Commands.AdjustInventory;

public sealed record AdjustInventoryCommand(
    Guid ProductId,
    int QuantityAdjustment,
    string Reason = "Manual adjustment") : IRequest<AdjustInventoryResponse>;

public sealed record AdjustInventoryResponse(
    Guid ProductId,
    int NewAvailableQuantity,
    int ReservedQuantity,
    string Message);
