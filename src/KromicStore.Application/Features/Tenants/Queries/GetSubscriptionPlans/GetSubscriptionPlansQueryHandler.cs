using MediatR;
using Microsoft.Extensions.Logging;
using KromicStore.Application.Features.Tenants.Abstractions;

namespace KromicStore.Application.Features.Tenants.Queries.GetSubscriptionPlans;

public sealed class GetSubscriptionPlansQueryHandler : IRequestHandler<GetSubscriptionPlansQuery, SubscriptionPlansResponse>
{
    private readonly ISubscriptionPlanRepository _planRepository;
    private readonly ILogger<GetSubscriptionPlansQueryHandler> _logger;

    public GetSubscriptionPlansQueryHandler(
        ISubscriptionPlanRepository planRepository,
        ILogger<GetSubscriptionPlansQueryHandler> logger)
    {
        _planRepository = planRepository ?? throw new ArgumentNullException(nameof(planRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<SubscriptionPlansResponse> Handle(
        GetSubscriptionPlansQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Retrieving subscription plans: Skip={Skip}, Take={Take}, IsActive={IsActive}",
            request.Skip, request.Take, request.IsActive);

        var (plans, totalCount) = await _planRepository.GetPaginatedAsync(
            request.Skip,
            request.Take,
            request.IsActive,
            cancellationToken);

        var dtos = plans.Select(p => new SubscriptionPlanDto
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            MonthlyPrice = p.MonthlyPrice,
            AnnualPrice = p.AnnualPrice,
            MaxProducts = p.MaxProducts,
            MaxCategories = p.MaxCategories,
            MaxStaff = p.MaxStaff,
            IsActive = p.IsActive
        }).ToList();

        return new SubscriptionPlansResponse
        {
            Plans = dtos,
            TotalCount = totalCount
        };
    }
}
