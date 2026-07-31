using MediatR;
using Microsoft.Extensions.Logging;
using KromicStore.Application.Features.Tenants.Abstractions;

namespace KromicStore.Application.Features.Tenants.Commands.UpdateStoreSettings;

public sealed class UpdateStoreSettingsCommandHandler : IRequestHandler<UpdateStoreSettingsCommand, UpdateStoreSettingsResponse>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly ILogger<UpdateStoreSettingsCommandHandler> _logger;

    public UpdateStoreSettingsCommandHandler(ITenantRepository tenantRepository, ILogger<UpdateStoreSettingsCommandHandler> logger)
    {
        _tenantRepository = tenantRepository ?? throw new ArgumentNullException(nameof(tenantRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<UpdateStoreSettingsResponse> Handle(UpdateStoreSettingsCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating store settings for tenant {TenantId}", request.TenantId);

        var tenant = await _tenantRepository.GetByIdAsync(request.TenantId, cancellationToken);
        if (tenant == null)
        {
            _logger.LogWarning("Tenant {TenantId} not found", request.TenantId);
            throw new InvalidOperationException($"Tenant {request.TenantId} not found.");
        }

        // Update tenant properties using domain methods
        if (!string.IsNullOrEmpty(request.StoreName))
            tenant.RenameStore(request.StoreName);

        // TODO: Create TenantSettings repository to persist additional fields (Email, Phone, Address, Currency, Timezone, Language)
        // These fields should be stored in TenantSettings entity instead of Tenant

        _logger.LogInformation("Store settings updated for tenant {TenantId}", request.TenantId);

        return new UpdateStoreSettingsResponse
        {
            TenantId = tenant.Id,
            Success = true,
            Message = "Store settings updated successfully"
        };
    }
}
