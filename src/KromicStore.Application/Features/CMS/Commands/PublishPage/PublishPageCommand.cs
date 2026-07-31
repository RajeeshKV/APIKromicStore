using MediatR;

namespace KromicStore.Application.Features.CMS.Commands.PublishPage;

/// <summary>
/// Command to publish a CMS page immediately.
/// </summary>
public sealed record PublishPageCommand(
    Guid PageId,
    Guid TenantId) : IRequest<PublishPageResponse>;

/// <summary>
/// Response from PublishPageCommand.
/// </summary>
public sealed record PublishPageResponse(
    Guid PageId,
    string Status,
    DateTime? PublishedDateUtc);
