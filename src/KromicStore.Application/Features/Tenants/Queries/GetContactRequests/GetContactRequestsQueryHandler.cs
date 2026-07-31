using MediatR;
using Microsoft.Extensions.Logging;
using KromicStore.Application.Features.Tenants.Abstractions;

namespace KromicStore.Application.Features.Tenants.Queries.GetContactRequests;

public sealed class GetContactRequestsQueryHandler : IRequestHandler<GetContactRequestsQuery, GetContactRequestsResponse>
{
    private readonly IContactRequestRepository _contactRepository;
    private readonly ILogger<GetContactRequestsQueryHandler> _logger;

    public GetContactRequestsQueryHandler(
        IContactRequestRepository contactRepository,
        ILogger<GetContactRequestsQueryHandler> logger)
    {
        _contactRepository = contactRepository ?? throw new ArgumentNullException(nameof(contactRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<GetContactRequestsResponse> Handle(
        GetContactRequestsQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving contact requests: Skip={Skip}, Take={Take}, Status={Status}",
            request.Skip, request.Take, request.StatusFilter);

        var (requests, totalCount) = await _contactRepository.GetPaginatedAsync(
            request.Skip,
            request.Take,
            request.StatusFilter,
            request.SearchTerm,
            cancellationToken);

        var unresolvedCount = await _contactRepository.GetUnresolvedCountAsync(cancellationToken);

        var dtos = requests.Select(r => new ContactRequestDto
        {
            Id = r.Id,
            Name = r.Name,
            Email = r.Email,
            Subject = r.Subject,
            Status = r.Status.ToString(),
            ReceivedOnUtc = r.ReceivedOnUtc,
            ReplyCount = r.Replies.Count
        }).ToList();

        return new GetContactRequestsResponse
        {
            Requests = dtos,
            TotalCount = totalCount,
            UnresolvedCount = unresolvedCount
        };
    }
}
