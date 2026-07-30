using MediatR;
using KromicStore.Application.Features.Tenants.Abstractions;
using Microsoft.Extensions.Logging;

namespace KromicStore.Application.Features.Tenants.Commands.UpdateTenant;

public sealed class UpdateTenantCommandHandler : IRequestHandler<UpdateTenantCommand, UpdateTenantResponse>
{
    private readonly ITenantRepository _repository;
    private readonly ILogger<UpdateTenantCommandHandler> _logger;

    public UpdateTenantCommandHandler(ITenantRepository repository, ILogger<UpdateTenantCommandHandler> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<UpdateTenantResponse> Handle(UpdateTenantCommand request, CancellationToken cancellationToken)
    {
        var tenant = await _repository.GetByIdAsync(request.TenantId, cancellationToken)
            ?? throw new InvalidOperationException($"Tenant '{request.TenantId}' not found.");

        // Update StoreName if provided
        if (!string.IsNullOrWhiteSpace(request.StoreName))
        {
            tenant.RenameStore(request.StoreName);
        }

        // Update OwnerUserId if provided
        if (request.OwnerUserId.HasValue && request.OwnerUserId.Value != Guid.Empty)
        {
            tenant.AssignOwner(request.OwnerUserId.Value);
        }

        _repository.Update(tenant);
        await _repository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Tenant updated: {TenantId}", request.TenantId);

        return new UpdateTenantResponse(
            TenantId: tenant.Id,
            StoreName: tenant.StoreName,
            OwnerUserId: tenant.OwnerUserId);
    }
}
