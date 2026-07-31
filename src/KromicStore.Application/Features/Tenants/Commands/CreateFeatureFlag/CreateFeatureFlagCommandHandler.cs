using MediatR;
using Microsoft.Extensions.Logging;
using KromicStore.Application.Features.Tenants.Abstractions;
using KromicStore.Domain.Tenants;

namespace KromicStore.Application.Features.Tenants.Commands.CreateFeatureFlag;

public sealed class CreateFeatureFlagCommandHandler : IRequestHandler<CreateFeatureFlagCommand, Unit>
{
    private readonly IFeatureFlagRepository _flagRepository;
    private readonly ILogger<CreateFeatureFlagCommandHandler> _logger;

    public CreateFeatureFlagCommandHandler(
        IFeatureFlagRepository flagRepository,
        ILogger<CreateFeatureFlagCommandHandler> logger)
    {
        _flagRepository = flagRepository ?? throw new ArgumentNullException(nameof(flagRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Unit> Handle(CreateFeatureFlagCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating feature flag: {Code}", request.Code);

        var flag = FeatureFlag.Create(request.Code, request.Name, request.Description, request.IsEnabled, request.Scope);
        _flagRepository.Add(flag);
        await _flagRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Feature flag {FlagId} created", flag.Id);
        return Unit.Value;
    }
}
