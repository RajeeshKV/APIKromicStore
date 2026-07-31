using MediatR;

namespace KromicStore.Application.CustomerPortal.Commands.SetDefaultAddress;

/// <summary>
/// Command to set an address as default shipping or billing address.
/// </summary>
public sealed class SetDefaultAddressCommand : IRequest<SetDefaultAddressResponse>
{
    public Guid AddressId { get; set; }
    public Guid CustomerId { get; set; }
    public bool IsShippingDefault { get; set; }
    public bool IsBillingDefault { get; set; }
}

public sealed class SetDefaultAddressResponse
{
    public Guid AddressId { get; set; }
    public bool IsDefaultShipping { get; set; }
    public bool IsDefaultBilling { get; set; }
    public DateTime ModifiedOnUtc { get; set; }
}
