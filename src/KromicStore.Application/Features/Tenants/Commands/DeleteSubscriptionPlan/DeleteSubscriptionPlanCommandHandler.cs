using MediatR;
using Microsoft.Extensions.Logging;
using KromicStore.Application.Features.Tenants.Abstractions;

namespace KromicStore.Application.Features.Tenants.Commands.DeleteSubscriptionPlan;

public sealed class DeleteSubscriptionPlanCommandHandler : IRequestHandler<DeleteSubscriptionPlanCommand, Unit>
{
    private readonly ISubscriptionPlanRepository _planRepository;
    private readonly ILogger<DeleteSubscriptionPlanCommandHandler> _logger;

    public DeleteSubscriptionPlanCommandHandler(
        ISubscriptionPlanRepository planRepository,
        ILogger<DeleteSubscriptionPlanCommandHandler> logger)
    {
        _planRepository = planRepository ?? throw new ArgumentNullException(nameof(planRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Unit> Handle(DeleteSubscriptionPlanCommand request, CancellationToken cancellationToken)
    {
        if (request.PlanId == Guid.Empty)
            throw new ArgumentException("Plan ID is required.", nameof(request.PlanId));

        _logger.LogInformation("Deleting subscription plan: {PlanId}", request.PlanId);

        var plan = await _planRepository.GetByIdAsync(request.PlanId, cancellationToken);
        if (plan == null)
            throw new InvalidOperationException($"Plan {request.PlanId} not found.");

        _planRepository.Remove(plan);
        await _planRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Subscription plan {PlanId} deleted successfully", request.PlanId);

        return Unit.Value;
    }
}
