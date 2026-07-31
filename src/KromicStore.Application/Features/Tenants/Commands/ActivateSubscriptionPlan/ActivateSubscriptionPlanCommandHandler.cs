using MediatR;
using Microsoft.Extensions.Logging;
using KromicStore.Application.Features.Tenants.Abstractions;

namespace KromicStore.Application.Features.Tenants.Commands.ActivateSubscriptionPlan;

public sealed class ActivateSubscriptionPlanCommandHandler : IRequestHandler<ActivateSubscriptionPlanCommand, Unit>
{
    private readonly ISubscriptionPlanRepository _planRepository;
    private readonly ILogger<ActivateSubscriptionPlanCommandHandler> _logger;

    public ActivateSubscriptionPlanCommandHandler(
        ISubscriptionPlanRepository planRepository,
        ILogger<ActivateSubscriptionPlanCommandHandler> logger)
    {
        _planRepository = planRepository ?? throw new ArgumentNullException(nameof(planRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Unit> Handle(ActivateSubscriptionPlanCommand request, CancellationToken cancellationToken)
    {
        if (request.PlanId == Guid.Empty)
            throw new ArgumentException("Plan ID is required.", nameof(request.PlanId));

        var plan = await _planRepository.GetByIdAsync(request.PlanId, cancellationToken);
        if (plan == null)
            throw new InvalidOperationException($"Plan {request.PlanId} not found.");

        plan.Activate();
        _planRepository.Update(plan);
        await _planRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Subscription plan {PlanId} activated", request.PlanId);
        return Unit.Value;
    }
}
