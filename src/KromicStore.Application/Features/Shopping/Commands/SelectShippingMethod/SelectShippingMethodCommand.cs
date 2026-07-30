using MediatR;

namespace KromicStore.Application.Features.Shopping.Commands.SelectShippingMethod;

/// <summary>
/// Command to select a shipping method for a checkout session.
/// </summary>
public sealed record SelectShippingMethodCommand(
    Guid CheckoutSessionId,
    string ShippingMethodId,
    decimal ShippingCost) : IRequest<SelectShippingMethodResponse>;

/// <summary>
/// Response for SelectShippingMethod command.
/// </summary>
public sealed record SelectShippingMethodResponse(
    Guid CheckoutSessionId,
    string ShippingMethodId,
    decimal ShippingCost,
    decimal Total);
