using MediatR;
using Microsoft.Extensions.Logging;
using KromicStore.Application.Features.Tenants.Abstractions;

namespace KromicStore.Application.Features.Tenants.Commands.SuspendTenant;

/// <summary>
/// Handler for SuspendTenantCommand.
/// Suspends an active tenant.
/// </summary>
public sealed class SuspendTenantCommandHandler : IRequestHandler<SuspendTenantCommand, SuspendTenantResponse>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly ILogger<SuspendTenantCommandHandler> _logger;

    public SuspendTenantCommandHandler(
        ITenantRepository tenantRepository,
        ILogger<SuspendTenantCommandHandler> logger)
    {
        _tenantRepository = tenantRepository ?? throw new ArgumentNullException(nameof(tenantRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<SuspendTenantResponse> Handle(
        SuspendTenantCommand request,
        CancellationToken cancellationToken)
    {
        if (request.TenantId == Guid.Empty)
        {
            throw new ArgumentException("TenantId cannot be empty", nameof(request.TenantId));
        }

        _logger.LogInformation("Suspending tenant {TenantId} with reason: {Reason}", request.TenantId, request.Reason);

        // Get tenant
        var tenant = await _tenantRepository.GetByIdAsync(request.TenantId, cancellationToken);
        if (tenant == null)
        {
            _logger.LogWarning("Tenant {TenantId} not found", request.TenantId);
            throw new InvalidOperationException($"Tenant {request.TenantId} not found.");
        }

        // Suspend tenant
        tenant.Suspend();

        // Update tenant
        _tenantRepository.Update(tenant);
        await _tenantRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Tenant {TenantId} suspended successfully", request.TenantId);

        return new SuspendTenantResponse
        {
            Id = tenant.Id,
            Name = tenant.Name,
            Status = tenant.Status.ToString(),
            UpdatedOnUtc = tenant.ModifiedOnUtc ?? DateTime.UtcNow
        };
    }
}
