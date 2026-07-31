using MediatR;

namespace KromicStore.Application.Features.CMS.Commands.UnpublishPage;

/// <summary>
/// Command to unpublish a CMS page (return to draft).
/// </summary>
public sealed record UnpublishPageCommand(
    Guid PageId,
    Guid TenantId) : IRequest<UnpublishPageResponse>;

/// <summary>
/// Response from UnpublishPageCommand.
/// </summary>
public sealed record UnpublishPageResponse(
    Guid PageId,
    string Status);
