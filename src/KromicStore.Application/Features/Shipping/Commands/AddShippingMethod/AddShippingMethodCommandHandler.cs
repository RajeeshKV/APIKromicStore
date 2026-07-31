using KromicStore.Application.Common.Abstractions;
using KromicStore.Application.Features.Shipping.Abstractions;
using KromicStore.Domain.Shipping.Entities;
using MediatR;

namespace KromicStore.Application.Features.Shipping.Commands.AddShippingMethod;

public sealed class AddShippingMethodCommandHandler : IRequestHandler<AddShippingMethodCommand, AddShippingMethodResponse>
{
    private readonly IShippingZoneRepository _zoneRepository;
    private readonly IShippingMethodRepository _methodRepository;
    private readonly ITenantContext _tenantContext;

    public AddShippingMethodCommandHandler(
        IShippingZoneRepository zoneRepository,
        IShippingMethodRepository methodRepository,
        ITenantContext tenantContext)
    {
        _zoneRepository = zoneRepository ?? throw new ArgumentNullException(nameof(zoneRepository));
        _methodRepository = methodRepository ?? throw new ArgumentNullException(nameof(methodRepository));
        _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
    }

    public async Task<AddShippingMethodResponse> Handle(AddShippingMethodCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId ?? throw new InvalidOperationException("Tenant context is required");
        
        // Verify zone exists
        var zone = await _zoneRepository.GetByIdAsync(request.ShippingZoneId, cancellationToken);
        if (zone == null)
            throw new InvalidOperationException($"Shipping zone {request.ShippingZoneId} not found");
        
        // Create shipping method
        var method = ShippingMethod.Create(
            tenantId,
            request.ShippingZoneId,
            request.Name,
            request.EstimatedDaysMin,
            request.EstimatedDaysMax,
            request.Description);
        
        // Add to repository and save
        _methodRepository.Add(method);
        await _methodRepository.SaveChangesAsync(cancellationToken);
        
        return new AddShippingMethodResponse
        {
            MethodId = method.Id,
            ZoneId = method.ShippingZoneId,
            Name = method.Name,
            IsActive = method.IsActive
        };
    }
}
