using MediatR;
using Microsoft.Extensions.Logging;
using KromicStore.Application.Features.Tenants.Abstractions;

namespace KromicStore.Application.Features.Tenants.Commands.ArchiveTenant;

public sealed class ArchiveTenantCommandHandler : IRequestHandler<ArchiveTenantCommand, ArchiveTenantResponse>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly ILogger<ArchiveTenantCommandHandler> _logger;

    public ArchiveTenantCommandHandler(
        ITenantRepository tenantRepository,
        ILogger<ArchiveTenantCommandHandler> logger)
    {
        _tenantRepository = tenantRepository ?? throw new ArgumentNullException(nameof(tenantRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ArchiveTenantResponse> Handle(
        ArchiveTenantCommand request,
        CancellationToken cancellationToken)
    {
        if (request.TenantId == Guid.Empty)
            throw new ArgumentException("TenantId cannot be empty", nameof(request.TenantId));

        _logger.LogInformation("Archiving tenant {TenantId}", request.TenantId);

        var tenant = await _tenantRepository.GetByIdAsync(request.TenantId, cancellationToken);
        if (tenant == null)
        {
            _logger.LogWarning("Tenant {TenantId} not found", request.TenantId);
            throw new InvalidOperationException($"Tenant {request.TenantId} not found.");
        }

        tenant.Archive();
        _tenantRepository.Update(tenant);
        await _tenantRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Tenant {TenantId} archived successfully", request.TenantId);

        return new ArchiveTenantResponse
        {
            Id = tenant.Id,
            Name = tenant.Name,
            Status = tenant.Status.ToString()
        };
    }
}
