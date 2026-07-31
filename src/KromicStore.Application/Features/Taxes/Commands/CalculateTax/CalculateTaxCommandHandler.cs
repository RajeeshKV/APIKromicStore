using KromicStore.Application.Features.Taxes.Abstractions;
using MediatR;

namespace KromicStore.Application.Features.Taxes.Commands.CalculateTax;

public sealed class CalculateTaxCommandHandler : IRequestHandler<CalculateTaxCommand, CalculateTaxResponse>
{
    private readonly ITaxRegionRepository _repository;

    public CalculateTaxCommandHandler(ITaxRegionRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task<CalculateTaxResponse> Handle(CalculateTaxCommand request, CancellationToken cancellationToken)
    {
        // Get tax region
        var region = await _repository.GetByIdAsync(request.TaxRegionId, cancellationToken);
        if (region == null)
            return new CalculateTaxResponse
            {
                Success = false,
                Message = "Tax region not found"
            };
        
        // Check if region is active
        if (!region.IsActive)
            return new CalculateTaxResponse
            {
                Success = false,
                Message = "Tax region is not active"
            };
        
        // Get tax rate
        var taxRate = region.GetTaxRate(request.ProductCategory);
        if (taxRate == 0)
            return new CalculateTaxResponse
            {
                Success = false,
                Message = $"No tax rule found for category: {request.ProductCategory}"
            };
        
        // Calculate tax amount
        // Note: taxRate is already a decimal between 0 and 1 (e.g., 0.15 for 15%)
        var taxAmount = request.OrderAmount * taxRate;
        
        return new CalculateTaxResponse
        {
            Success = true,
            Message = "Tax calculated successfully",
            TaxAmount = taxAmount,
            TaxRate = taxRate
        };
    }
}
