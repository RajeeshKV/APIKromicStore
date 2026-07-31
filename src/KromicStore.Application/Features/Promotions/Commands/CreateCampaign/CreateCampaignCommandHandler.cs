using KromicStore.Application.Common.Abstractions;
using KromicStore.Application.Features.Promotions.Abstractions;
using KromicStore.Domain.Promotions.Entities;
using MediatR;

namespace KromicStore.Application.Features.Promotions.Commands.CreateCampaign;

public sealed class CreateCampaignCommandHandler : IRequestHandler<CreateCampaignCommand, CreateCampaignResponse>
{
    private readonly IPromotionRepository _repository;
    private readonly ITenantContext _tenantContext;

    public CreateCampaignCommandHandler(IPromotionRepository repository, ITenantContext tenantContext)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
    }

    public async Task<CreateCampaignResponse> Handle(CreateCampaignCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId ?? throw new InvalidOperationException("Tenant context is required");
        
        // Create campaign
        var campaign = Campaign.Create(tenantId, request.Name, request.StartDateUtc, request.EndDateUtc, request.Description);
        
        // Add discounts
        foreach (var discountId in request.DiscountIds)
        {
            var discount = await _repository.GetDiscountByIdAsync(discountId, cancellationToken);
            if (discount != null)
            {
                campaign.AddDiscount(discountId);
            }
        }
        
        // Add to repository and save
        _repository.AddCampaign(campaign);
        await _repository.SaveChangesAsync(cancellationToken);
        
        return new CreateCampaignResponse
        {
            CampaignId = campaign.Id,
            Name = campaign.Name,
            DiscountCount = campaign.DiscountIds.Count,
            IsActive = campaign.IsValid()
        };
    }
}
