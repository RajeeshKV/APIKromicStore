using KromicStore.Application.Features.Shipping.Abstractions;
using MediatR;

namespace KromicStore.Application.Features.Shipping.Commands.CalculateShippingCost;

public sealed class CalculateShippingCostCommandHandler : IRequestHandler<CalculateShippingCostCommand, CalculateShippingCostResponse>
{
    private readonly IShippingMethodRepository _repository;

    public CalculateShippingCostCommandHandler(IShippingMethodRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task<CalculateShippingCostResponse> Handle(CalculateShippingCostCommand request, CancellationToken cancellationToken)
    {
        // Get shipping method
        var method = await _repository.GetByIdAsync(request.ShippingMethodId, cancellationToken);
        if (method == null)
            return new CalculateShippingCostResponse
            {
                Success = false,
                Message = "Shipping method not found"
            };
        
        // Check if method is active
        if (!method.IsActive)
            return new CalculateShippingCostResponse
            {
                Success = false,
                Message = "Shipping method is not available"
            };
        
        // Calculate cost
        var cost = method.CalculateShippingCost(request.Weight, request.OrderValue);
        
        if (cost == null)
            return new CalculateShippingCostResponse
            {
                Success = false,
                Message = "No applicable shipping rate found for the given weight/value"
            };
        
        return new CalculateShippingCostResponse
        {
            Success = true,
            Message = "Shipping cost calculated successfully",
            ShippingCost = cost
        };
    }
}
