using MediatR;

namespace KromicStore.Application.Features.Shopping.Commands.UpdateShippingAddress;

/// <summary>
/// Command to update the shipping address for a checkout session.
/// </summary>
public sealed record UpdateShippingAddressCommand(
    Guid CheckoutSessionId,
    string Street,
    string City,
    string State,
    string PostalCode,
    string Country) : IRequest<UpdateShippingAddressResponse>;

/// <summary>
/// Response for UpdateShippingAddress command.
/// </summary>
public sealed record UpdateShippingAddressResponse(
    Guid CheckoutSessionId,
    bool ShippingAddressUpdated,
    string FullAddress);
