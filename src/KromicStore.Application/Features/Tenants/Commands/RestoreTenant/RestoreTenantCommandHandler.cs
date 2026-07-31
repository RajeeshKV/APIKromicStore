using MediatR;
using Microsoft.Extensions.Logging;
using KromicStore.Application.Features.Tenants.Abstractions;

namespace KromicStore.Application.Features.Tenants.Commands.RestoreTenant;

public sealed class RestoreTenantCommandHandler : IRequestHandler<RestoreTenantCommand, RestoreTenantResponse>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly ILogger<RestoreTenantCommandHandler> _logger;

    public RestoreTenantCommandHandler(
        ITenantRepository tenantRepository,
        ILogger<RestoreTenantCommandHandler> logger)
    {
        _tenantRepository = tenantRepository ?? throw new ArgumentNullException(nameof(tenantRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<RestoreTenantResponse> Handle(
        RestoreTenantCommand request,
        CancellationToken cancellationToken)
    {
        if (request.TenantId == Guid.Empty)
            throw new ArgumentException("TenantId cannot be empty", nameof(request.TenantId));

        _logger.LogInformation("Restoring tenant {TenantId}", request.TenantId);

        var tenant = await _tenantRepository.GetByIdAsync(request.TenantId, cancellationToken);
        if (tenant == null)
        {
            _logger.LogWarning("Tenant {TenantId} not found", request.TenantId);
            throw new InvalidOperationException($"Tenant {request.TenantId} not found.");
        }

        if (tenant.Status != Domain.Tenants.TenantStatus.Archived)
        {
            _logger.LogWarning("Cannot restore tenant {TenantId} - status is {Status}", request.TenantId, tenant.Status);
            throw new InvalidOperationException($"Only archived tenants can be restored. Current status: {tenant.Status}");
        }

        tenant.Activate();
        _tenantRepository.Update(tenant);
        await _tenantRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Tenant {TenantId} restored successfully", request.TenantId);

        return new RestoreTenantResponse
        {
            Id = tenant.Id,
            Name = tenant.Name,
            Status = tenant.Status.ToString()
        };
    }
}
