using MediatR;

namespace KromicStore.Application.Features.Shipping.Commands.AddShippingMethod;

public sealed class AddShippingMethodCommand : IRequest<AddShippingMethodResponse>
{
    public Guid ShippingZoneId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Code { get; set; } = string.Empty;
    public int EstimatedDaysMin { get; set; }
    public int EstimatedDaysMax { get; set; }
}

public sealed class AddShippingMethodResponse
{
    public Guid MethodId { get; set; }
    public Guid ZoneId { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
