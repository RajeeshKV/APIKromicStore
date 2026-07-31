using MediatR;

namespace KromicStore.Application.Features.Tenants.Queries.GetPublishStatus;

public sealed class GetPublishStatusQuery : IRequest<PublishStatusResponse>
{
    public Guid TenantId { get; set; }
}

public sealed class PublishStatusResponse
{
    public bool IsPublished { get; set; }
    public DateTime? PublishedOnUtc { get; set; }
    public string StoreUrl { get; set; } = string.Empty;
    public string Status { get; set; } = "Draft";
}
