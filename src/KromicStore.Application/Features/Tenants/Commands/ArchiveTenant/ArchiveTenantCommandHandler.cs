using MediatR;
using KromicStore.Application.Features.Tenants.Abstractions;
using Microsoft.Extensions.Logging;

namespace KromicStore.Application.Features.Tenants.Commands.ArchiveTenant;

public sealed class ArchiveTenantCommandHandler : IRequestHandler<ArchiveTenantCommand, Unit>
{
    private readonly ITenantRepository _repository;
    private readonly ILogger<ArchiveTenantCommandHandler> _logger;

    public ArchiveTenantCommandHandler(ITenantRepository repository, ILogger<ArchiveTenantCommandHandler> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Unit> Handle(ArchiveTenantCommand request, CancellationToken cancellationToken)
    {
        var tenant = await _repository.GetByIdAsync(request.TenantId, cancellationToken)
            ?? throw new InvalidOperationException($"Tenant '{request.TenantId}' not found.");

        tenant.Archive();
        _repository.Update(tenant);
        await _repository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Tenant archived: {TenantId}", request.TenantId);
        
        return Unit.Value;
    }
}
