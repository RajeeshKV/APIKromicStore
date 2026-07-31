using MediatR;
using KromicStore.Domain.Tenants;

namespace KromicStore.Application.Features.Tenants.Queries.GetContactRequests;

public sealed class GetContactRequestsQuery : IRequest<GetContactRequestsResponse>
{
    public int Skip { get; set; } = 0;
    public int Take { get; set; } = 20;
    public ContactRequestStatus? StatusFilter { get; set; }
    public string? SearchTerm { get; set; }
}

public sealed class ContactRequestDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime ReceivedOnUtc { get; set; }
    public int ReplyCount { get; set; }
}

public sealed class GetContactRequestsResponse
{
    public List<ContactRequestDto> Requests { get; set; } = new();
    public int TotalCount { get; set; }
    public int UnresolvedCount { get; set; }
}
