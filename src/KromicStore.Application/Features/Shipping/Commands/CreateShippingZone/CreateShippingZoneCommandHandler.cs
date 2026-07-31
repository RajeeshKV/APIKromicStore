using KromicStore.Application.Common.Abstractions;
using KromicStore.Application.Features.Shipping.Abstractions;
using KromicStore.Domain.Shipping.Entities;
using MediatR;

namespace KromicStore.Application.Features.Shipping.Commands.CreateShippingZone;

public sealed class CreateShippingZoneCommandHandler : IRequestHandler<CreateShippingZoneCommand, CreateShippingZoneResponse>
{
    private readonly IShippingZoneRepository _repository;
    private readonly ITenantContext _tenantContext;

    public CreateShippingZoneCommandHandler(IShippingZoneRepository repository, ITenantContext tenantContext)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
    }

    public async Task<CreateShippingZoneResponse> Handle(CreateShippingZoneCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId ?? throw new InvalidOperationException("Tenant context is required");
        
        // Create the shipping zone
        var zone = ShippingZone.Create(tenantId, request.Name, request.Description);
        
        // Add countries
        foreach (var country in request.Countries)
        {
            zone.AddCountry(country);
        }
        
        // Add to repository and save
        _repository.Add(zone);
        await _repository.SaveChangesAsync(cancellationToken);
        
        return new CreateShippingZoneResponse
        {
            ZoneId = zone.Id,
            Name = zone.Name,
            IsActive = zone.IsActive
        };
    }
}
