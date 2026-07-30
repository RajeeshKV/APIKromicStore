using MediatR;

namespace KromicStore.Application.Features.Shopping.Commands.UpdateBillingAddress;

/// <summary>
/// Command to update the billing address for a checkout session.
/// </summary>
public sealed record UpdateBillingAddressCommand(
    Guid CheckoutSessionId,
    string Street,
    string City,
    string State,
    string PostalCode,
    string Country) : IRequest<UpdateBillingAddressResponse>;

/// <summary>
/// Response for UpdateBillingAddress command.
/// </summary>
public sealed record UpdateBillingAddressResponse(
    Guid CheckoutSessionId,
    bool BillingAddressUpdated,
    string FullAddress);
