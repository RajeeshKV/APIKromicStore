using MediatR;

namespace KromicStore.Application.CustomerPortal.Commands.AddAddress;

/// <summary>
/// Command to add a new address to customer's address book.
/// </summary>
public sealed class AddAddressCommand : IRequest<AddAddressResponse>
{
    public Guid CustomerId { get; set; }
    public string Label { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string StateCode { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string CountryCode { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public bool IsShippingAddress { get; set; } = true;
    public bool IsBillingAddress { get; set; }
}

public sealed class AddAddressResponse
{
    public Guid AddressId { get; set; }
    public string FormattedAddress { get; set; } = string.Empty;
    public DateTime CreatedOnUtc { get; set; }
}
