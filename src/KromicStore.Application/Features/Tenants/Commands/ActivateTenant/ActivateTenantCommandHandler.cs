using MediatR;
using Microsoft.Extensions.Logging;
using KromicStore.Application.Features.Tenants.Abstractions;

namespace KromicStore.Application.Features.Tenants.Commands.ActivateTenant;

/// <summary>
/// Handler for ActivateTenantCommand.
/// Activates a suspended or provisioning tenant.
/// </summary>
public sealed class ActivateTenantCommandHandler : IRequestHandler<ActivateTenantCommand, ActivateTenantResponse>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly ILogger<ActivateTenantCommandHandler> _logger;

    public ActivateTenantCommandHandler(
        ITenantRepository tenantRepository,
        ILogger<ActivateTenantCommandHandler> logger)
    {
        _tenantRepository = tenantRepository ?? throw new ArgumentNullException(nameof(tenantRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ActivateTenantResponse> Handle(
        ActivateTenantCommand request,
        CancellationToken cancellationToken)
    {
        if (request.TenantId == Guid.Empty)
        {
            throw new ArgumentException("TenantId cannot be empty", nameof(request.TenantId));
        }

        _logger.LogInformation("Activating tenant {TenantId}", request.TenantId);

        // Get tenant
        var tenant = await _tenantRepository.GetByIdAsync(request.TenantId, cancellationToken);
        if (tenant == null)
        {
            _logger.LogWarning("Tenant {TenantId} not found", request.TenantId);
            throw new InvalidOperationException($"Tenant {request.TenantId} not found.");
        }

        // Activate tenant
        tenant.Activate();

        // Update tenant
        _tenantRepository.Update(tenant);
        await _tenantRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Tenant {TenantId} activated successfully", request.TenantId);

        return new ActivateTenantResponse
        {
            Id = tenant.Id,
            Name = tenant.Name,
            Status = tenant.Status.ToString(),
            UpdatedOnUtc = tenant.ModifiedOnUtc ?? DateTime.UtcNow
        };
    }
}
