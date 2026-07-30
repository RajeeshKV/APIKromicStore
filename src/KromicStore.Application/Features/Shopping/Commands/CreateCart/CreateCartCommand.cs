using MediatR;

namespace KromicStore.Application.Features.Shopping.Commands.CreateCart;

/// <summary>
/// Command to create a new shopping cart for a customer or guest.
/// </summary>
public sealed record CreateCartCommand(
    Guid? CustomerId = null,
    string? AnonymousSessionId = null,
    string Currency = "USD") : IRequest<CreateCartResponse>;

/// <summary>
/// Response for CreateCart command.
/// </summary>
public sealed record CreateCartResponse(
    Guid CartId,
    Guid? CustomerId,
    string? AnonymousSessionId,
    string Currency,
    int ItemsCount,
    decimal SubTotal);
