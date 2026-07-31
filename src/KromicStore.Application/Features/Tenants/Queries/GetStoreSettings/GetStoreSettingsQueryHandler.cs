using MediatR;
using Microsoft.Extensions.Logging;
using KromicStore.Application.Features.Tenants.Abstractions;

namespace KromicStore.Application.Features.Tenants.Queries.GetStoreSettings;

public sealed class GetStoreSettingsQueryHandler : IRequestHandler<GetStoreSettingsQuery, StoreSettingsResponse>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly ILogger<GetStoreSettingsQueryHandler> _logger;

    public GetStoreSettingsQueryHandler(ITenantRepository tenantRepository, ILogger<GetStoreSettingsQueryHandler> logger)
    {
        _tenantRepository = tenantRepository ?? throw new ArgumentNullException(nameof(tenantRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<StoreSettingsResponse> Handle(GetStoreSettingsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving store settings for tenant {TenantId}", request.TenantId);

        var tenant = await _tenantRepository.GetByIdAsync(request.TenantId, cancellationToken);
        if (tenant == null)
            throw new InvalidOperationException($"Tenant {request.TenantId} not found.");

        return new StoreSettingsResponse
        {
            TenantId = tenant.Id,
            StoreName = tenant.StoreName,
            Description = tenant.Name,
            Email = null, // Would need StoreSettings entity
            PhoneNumber = null,
            WhatsAppNumber = null,
            Address = null,
            CurrencyCode = "USD",
            Timezone = "UTC",
            Language = "en"
        };
    }
}
