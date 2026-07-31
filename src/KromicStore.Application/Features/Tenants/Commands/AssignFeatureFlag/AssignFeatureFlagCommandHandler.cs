using MediatR;
using Microsoft.Extensions.Logging;
using KromicStore.Application.Features.Tenants.Abstractions;
using KromicStore.Domain.Tenants;

namespace KromicStore.Application.Features.Tenants.Commands.AssignFeatureFlag;

public sealed class AssignFeatureFlagCommandHandler : IRequestHandler<AssignFeatureFlagCommand, Unit>
{
    private readonly IFeatureFlagRepository _flagRepository;
    private readonly ILogger<AssignFeatureFlagCommandHandler> _logger;

    public AssignFeatureFlagCommandHandler(
        IFeatureFlagRepository flagRepository,
        ILogger<AssignFeatureFlagCommandHandler> logger)
    {
        _flagRepository = flagRepository ?? throw new ArgumentNullException(nameof(flagRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Unit> Handle(AssignFeatureFlagCommand request, CancellationToken cancellationToken)
    {
        var flag = await _flagRepository.GetByIdAsync(request.FeatureFlagId, cancellationToken);
        if (flag == null)
            throw new InvalidOperationException($"Feature flag {request.FeatureFlagId} not found.");

        var assignment = FeatureFlagAssignment.Create(
            request.FeatureFlagId,
            request.AssignmentType,
            request.AssignedToEntityId,
            request.IsEnabled);

        flag.AddAssignment(assignment);
        _flagRepository.Update(flag);
        await _flagRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Feature flag {FlagId} assigned to {EntityType} {EntityId}",
            request.FeatureFlagId, request.AssignmentType, request.AssignedToEntityId);

        return Unit.Value;
    }
}
