using MediatR;
using Microsoft.Extensions.Logging;
using KromicStore.Application.Features.Tenants.Abstractions;

namespace KromicStore.Application.Features.Tenants.Commands.UpdateSubscriptionPlan;

public sealed class UpdateSubscriptionPlanCommandHandler : IRequestHandler<UpdateSubscriptionPlanCommand, UpdateSubscriptionPlanResponse>
{
    private readonly ISubscriptionPlanRepository _planRepository;
    private readonly ILogger<UpdateSubscriptionPlanCommandHandler> _logger;

    public UpdateSubscriptionPlanCommandHandler(
        ISubscriptionPlanRepository planRepository,
        ILogger<UpdateSubscriptionPlanCommandHandler> logger)
    {
        _planRepository = planRepository ?? throw new ArgumentNullException(nameof(planRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<UpdateSubscriptionPlanResponse> Handle(
        UpdateSubscriptionPlanCommand request,
        CancellationToken cancellationToken)
    {
        if (request.PlanId == Guid.Empty)
            throw new ArgumentException("Plan ID is required.", nameof(request.PlanId));

        _logger.LogInformation("Updating subscription plan: {PlanId}", request.PlanId);

        var plan = await _planRepository.GetByIdAsync(request.PlanId, cancellationToken);
        if (plan == null)
            throw new InvalidOperationException($"Plan {request.PlanId} not found.");

        plan.Update(request.Name, request.Description, request.MonthlyPrice, request.AnnualPrice);
        plan.SetFeatureLimits(request.MaxProducts, request.MaxCategories, 10, 5, 999_999);

        _planRepository.Update(plan);
        await _planRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Subscription plan {PlanId} updated successfully", plan.Id);

        return new UpdateSubscriptionPlanResponse
        {
            Id = plan.Id,
            Name = plan.Name
        };
    }
}
