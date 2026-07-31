using MediatR;

namespace KromicStore.Application.CustomerPortal.Commands.DeleteAddress;

/// <summary>
/// Command to soft-delete a customer address.
/// </summary>
public sealed class DeleteAddressCommand : IRequest<DeleteAddressResponse>
{
    public Guid AddressId { get; set; }
    public Guid CustomerId { get; set; }
}

public sealed class DeleteAddressResponse
{
    public Guid AddressId { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime DeletedOnUtc { get; set; }
}
