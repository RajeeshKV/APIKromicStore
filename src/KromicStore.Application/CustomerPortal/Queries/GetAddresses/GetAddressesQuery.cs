using MediatR;

namespace KromicStore.Application.CustomerPortal.Queries.GetAddresses;

/// <summary>
/// Query to retrieve customer addresses with optional filtering.
/// </summary>
public sealed class GetAddressesQuery : IRequest<GetAddressesResponse>
{
    public Guid CustomerId { get; set; }
    public bool? IsActive { get; set; }
    public bool? OnlyShipping { get; set; }
    public bool? OnlyBilling { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public sealed class AddressDto
{
    public Guid AddressId { get; set; }
    public string Label { get; set; } = string.Empty;
    public string FormattedAddress { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public bool IsShippingAddress { get; set; }
    public bool IsBillingAddress { get; set; }
    public bool IsDefaultShipping { get; set; }
    public bool IsDefaultBilling { get; set; }
    public bool IsActive { get; set; }
}

public sealed class GetAddressesResponse
{
    public List<AddressDto> Addresses { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}
