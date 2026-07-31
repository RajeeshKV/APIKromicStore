using MediatR;

namespace KromicStore.Application.Features.Shipping.Commands.CalculateShippingCost;

public sealed class CalculateShippingCostCommand : IRequest<CalculateShippingCostResponse>
{
    public Guid ShippingMethodId { get; set; }
    public decimal Weight { get; set; }
    public decimal OrderValue { get; set; }
}

public sealed class CalculateShippingCostResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public decimal? ShippingCost { get; set; }
}
