using MediatR;

namespace KromicStore.Application.Features.Shipping.Commands.CreateShippingZone;

/// <summary>
/// Command to create a new shipping zone.
/// </summary>
public sealed class CreateShippingZoneCommand : IRequest<CreateShippingZoneResponse>
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<string> Countries { get; set; } = [];
}

public sealed class CreateShippingZoneResponse
{
    public Guid ZoneId { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
