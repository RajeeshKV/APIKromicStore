using MediatR;

namespace KromicStore.Application.Features.Promotions.Commands.CreateCampaign;

public sealed class CreateCampaignCommand : IRequest<CreateCampaignResponse>
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<Guid> DiscountIds { get; set; } = [];
    public DateTime StartDateUtc { get; set; }
    public DateTime EndDateUtc { get; set; }
}

public sealed class CreateCampaignResponse
{
    public Guid CampaignId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int DiscountCount { get; set; }
    public bool IsActive { get; set; }
}
