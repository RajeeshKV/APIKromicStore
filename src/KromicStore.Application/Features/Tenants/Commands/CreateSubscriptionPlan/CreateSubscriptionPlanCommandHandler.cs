using MediatR;
using Microsoft.Extensions.Logging;
using KromicStore.Application.Features.Tenants.Abstractions;
using KromicStore.Domain.Tenants;

namespace KromicStore.Application.Features.Tenants.Commands.CreateSubscriptionPlan;

public sealed class CreateSubscriptionPlanCommandHandler : IRequestHandler<CreateSubscriptionPlanCommand, CreateSubscriptionPlanResponse>
{
    private readonly ISubscriptionPlanRepository _planRepository;
    private readonly ILogger<CreateSubscriptionPlanCommandHandler> _logger;

    public CreateSubscriptionPlanCommandHandler(
        ISubscriptionPlanRepository planRepository,
        ILogger<CreateSubscriptionPlanCommandHandler> logger)
    {
        _planRepository = planRepository ?? throw new ArgumentNullException(nameof(planRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<CreateSubscriptionPlanResponse> Handle(
        CreateSubscriptionPlanCommand request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("Plan name is required.", nameof(request.Name));

        if (request.MonthlyPrice < 0)
            throw new ArgumentException("Monthly price cannot be negative.", nameof(request.MonthlyPrice));

        _logger.LogInformation("Creating subscription plan: {PlanName}", request.Name);

        var plan = SubscriptionPlan.Create(request.Name, request.Description, request.MonthlyPrice);

        plan.SetFeatureLimits(
            request.MaxProducts,
            request.MaxCategories,
            request.MaxCollections,
            request.MaxStaff,
            999_999);

        plan.SetStorageLimits(
            request.MaxStorageBytes,
            request.MaxEmailsPerMonth,
            100_000);

        plan.SetCapabilities(
            canCustomizeDomain: request.CanCustomizeDomain,
            canUseCustomTheme: request.CanUseCustomTheme);

        if (request.AnnualPrice.HasValue)
        {
            plan.Update(request.Name, request.Description, request.MonthlyPrice, request.AnnualPrice);
        }

        _planRepository.Add(plan);
        await _planRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Subscription plan {PlanId} created successfully", plan.Id);

        return new CreateSubscriptionPlanResponse
        {
            Id = plan.Id,
            Name = plan.Name,
            IsActive = plan.IsActive
        };
    }
}
