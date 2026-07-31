using MediatR;
using Microsoft.Extensions.Logging;
using KromicStore.Application.Features.Tenants.Abstractions;

namespace KromicStore.Application.Features.Tenants.Commands.DeactivateSubscriptionPlan;

public sealed class DeactivateSubscriptionPlanCommandHandler : IRequestHandler<DeactivateSubscriptionPlanCommand, Unit>
{
    private readonly ISubscriptionPlanRepository _planRepository;
    private readonly ILogger<DeactivateSubscriptionPlanCommandHandler> _logger;

    public DeactivateSubscriptionPlanCommandHandler(
        ISubscriptionPlanRepository planRepository,
        ILogger<DeactivateSubscriptionPlanCommandHandler> logger)
    {
        _planRepository = planRepository ?? throw new ArgumentNullException(nameof(planRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Unit> Handle(DeactivateSubscriptionPlanCommand request, CancellationToken cancellationToken)
    {
        if (request.PlanId == Guid.Empty)
            throw new ArgumentException("Plan ID is required.", nameof(request.PlanId));

        var plan = await _planRepository.GetByIdAsync(request.PlanId, cancellationToken);
        if (plan == null)
            throw new InvalidOperationException($"Plan {request.PlanId} not found.");

        plan.Deactivate();
        _planRepository.Update(plan);
        await _planRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Subscription plan {PlanId} deactivated", request.PlanId);
        return Unit.Value;
    }
}
