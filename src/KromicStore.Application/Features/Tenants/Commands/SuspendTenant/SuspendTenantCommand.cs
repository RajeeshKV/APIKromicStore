using MediatR;
using KromicStore.Application.Features.Tenants.Abstractions;
using Microsoft.Extensions.Logging;

namespace KromicStore.Application.Features.Tenants.Commands.SuspendTenant;

public sealed record SuspendTenantCommand(Guid TenantId) : IRequest<Unit>;

public sealed class SuspendTenantCommandHandler : IRequestHandler<SuspendTenantCommand, Unit>
{
    private readonly ITenantRepository _repository;
    private readonly ILogger<SuspendTenantCommandHandler> _logger;

    public SuspendTenantCommandHandler(ITenantRepository repository, ILogger<SuspendTenantCommandHandler> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Unit> Handle(SuspendTenantCommand request, CancellationToken cancellationToken)
    {
        var tenant = await _repository.GetByIdAsync(request.TenantId, cancellationToken)
            ?? throw new InvalidOperationException($"Tenant '{request.TenantId}' not found.");

        tenant.Suspend();
        _repository.Update(tenant);
        await _repository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Tenant suspended: {TenantId}", request.TenantId);
        
        return Unit.Value;
    }
}
