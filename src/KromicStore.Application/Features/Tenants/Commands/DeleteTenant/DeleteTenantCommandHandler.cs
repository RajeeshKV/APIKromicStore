using MediatR;
using Microsoft.Extensions.Logging;
using KromicStore.Application.Features.Tenants.Abstractions;

namespace KromicStore.Application.Features.Tenants.Commands.DeleteTenant;

public sealed class DeleteTenantCommandHandler : IRequestHandler<DeleteTenantCommand, Unit>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly ILogger<DeleteTenantCommandHandler> _logger;

    public DeleteTenantCommandHandler(
        ITenantRepository tenantRepository,
        ILogger<DeleteTenantCommandHandler> logger)
    {
        _tenantRepository = tenantRepository ?? throw new ArgumentNullException(nameof(tenantRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Unit> Handle(DeleteTenantCommand request, CancellationToken cancellationToken)
    {
        if (request.TenantId == Guid.Empty)
            throw new ArgumentException("TenantId cannot be empty", nameof(request.TenantId));

        _logger.LogInformation("Deleting tenant {TenantId} (HardDelete: {HardDelete})", request.TenantId, request.HardDelete);

        var tenant = await _tenantRepository.GetByIdAsync(request.TenantId, cancellationToken);
        if (tenant == null)
        {
            _logger.LogWarning("Tenant {TenantId} not found", request.TenantId);
            throw new InvalidOperationException($"Tenant {request.TenantId} not found.");
        }

        if (request.HardDelete)
        {
            _tenantRepository.Update(tenant); // Mark for hard delete
        }
        else
        {
            // Soft delete via AuditableEntity.SoftDelete
            tenant.Archive(); // Archive first as a softer approach
            _tenantRepository.Update(tenant);
        }

        await _tenantRepository.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Tenant {TenantId} deleted successfully", request.TenantId);

        return Unit.Value;
    }
}
