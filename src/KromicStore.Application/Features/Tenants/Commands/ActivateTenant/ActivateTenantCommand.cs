using MediatR;
using KromicStore.Application.Features.Tenants.Abstractions;
using Microsoft.Extensions.Logging;

namespace KromicStore.Application.Features.Tenants.Commands.ActivateTenant;

public sealed record ActivateTenantCommand(Guid TenantId) : IRequest<Unit>;

public sealed class ActivateTenantCommandHandler : IRequestHandler<ActivateTenantCommand, Unit>
{
    private readonly ITenantRepository _repository;
    private readonly ILogger<ActivateTenantCommandHandler> _logger;

    public ActivateTenantCommandHandler(ITenantRepository repository, ILogger<ActivateTenantCommandHandler> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Unit> Handle(ActivateTenantCommand request, CancellationToken cancellationToken)
    {
        var tenant = await _repository.GetByIdAsync(request.TenantId, cancellationToken)
            ?? throw new InvalidOperationException($"Tenant '{request.TenantId}' not found.");

        tenant.Activate();
        _repository.Update(tenant);
        await _repository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Tenant activated: {TenantId}", request.TenantId);
        
        return Unit.Value;
    }
}
