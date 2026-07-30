using MediatR;

namespace KromicStore.Application.Features.Shopping.Commands.ClearCart;

/// <summary>
/// Command to clear all items from a shopping cart.
/// </summary>
public sealed record ClearCartCommand(Guid CartId) : IRequest<ClearCartResponse>;

/// <summary>
/// Response for ClearCart command.
/// </summary>
public sealed record ClearCartResponse(
    Guid CartId,
    int PreviousItemsCount,
    decimal PreviousSubTotal,
    bool CartNowEmpty);
