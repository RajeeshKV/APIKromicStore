using MediatR;

namespace KromicStore.Application.CustomerPortal.Commands.UpdateAddress;

/// <summary>
/// Command to update an existing customer address.
/// </summary>
public sealed class UpdateAddressCommand : IRequest<UpdateAddressResponse>
{
    public Guid AddressId { get; set; }
    public Guid CustomerId { get; set; }
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string StateCode { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
}

public sealed class UpdateAddressResponse
{
    public Guid AddressId { get; set; }
    public string FormattedAddress { get; set; } = string.Empty;
    public DateTime ModifiedOnUtc { get; set; }
}
