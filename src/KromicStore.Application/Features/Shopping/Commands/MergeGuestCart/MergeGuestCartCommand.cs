using MediatR;

namespace KromicStore.Application.Features.Shopping.Commands.MergeGuestCart;

/// <summary>
/// Command to merge a guest cart into a customer cart when guest logs in.
/// This handles the transition from anonymous shopping to authenticated shopping.
/// </summary>
public sealed record MergeGuestCartCommand(
    Guid CustomerId,
    string AnonymousSessionId) : IRequest<MergeGuestCartResponse>;

/// <summary>
/// DTO for merged cart item in the response.
/// </summary>
public sealed record MergedCartItemDto(
    Guid ProductId,
    Guid? VariantId,
    int Quantity,
    decimal UnitPrice,
    decimal LineTotal);

/// <summary>
/// Response for MergeGuestCart command.
/// </summary>
public sealed record MergeGuestCartResponse(
    Guid MergedCartId,
    Guid CustomerId,
    int ItemsMerged,
    int ItemsInCustomerCart,
    int TotalItems,
    decimal MergedSubTotal,
    string Status);
