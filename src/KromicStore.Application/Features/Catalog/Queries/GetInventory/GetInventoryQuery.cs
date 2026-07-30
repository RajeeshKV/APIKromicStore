using MediatR;

namespace KromicStore.Application.Features.Catalog.Queries.GetInventory;

/// <summary>
/// Query to retrieve inventory information for a product.
/// </summary>
public sealed record GetInventoryQuery(Guid ProductId) : IRequest<GetInventoryResponse>;

/// <summary>
/// Data transfer object for inventory in query response.
/// </summary>
public sealed record InventoryDto(
    Guid ProductId,
    string Sku,
    int QuantityOnHand,
    int ReorderLevel,
    int QuantityReserved,
    int AvailableQuantity,
    bool IsInStock,
    bool IsBelowReorderLevel,
    DateTime? LastAdjustedAtUtc);

/// <summary>
/// Response for GetInventory query.
/// </summary>
public sealed record GetInventoryResponse(InventoryDto? Data);
